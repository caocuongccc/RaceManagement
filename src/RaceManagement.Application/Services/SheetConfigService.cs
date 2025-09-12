using Microsoft.Extensions.Logging;
using RaceManagement.Core.Entities;
using RaceManagement.Core.Interfaces;
using RaceManagement.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Application.Services
{
    public class SheetConfigService : ISheetConfigService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGoogleSheetsService _googleSheetsService;
        private readonly ILogger<SheetConfigService> _logger;

        public SheetConfigService(
            IUnitOfWork unitOfWork,
            IGoogleSheetsService googleSheetsService,
            ILogger<SheetConfigService> logger)
        {
            _unitOfWork = unitOfWork;
            _googleSheetsService = googleSheetsService;
            _logger = logger;
        }

        public async Task<SheetConfigDto> CreateSheetConfigAsync(CreateSheetConfigDto dto)
        {
            try
            {
                // Verify credential exists and is active
                var credential = await _unitOfWork.Credentials.GetByIdAsync(dto.CredentialId);
                if (credential == null || !credential.IsActive)
                {
                    throw new ArgumentException("Credential not found or inactive");
                }

                // Check for duplicates
                var nameExists = await _unitOfWork.SheetConfigs.IsNameExistsAsync(dto.Name, dto.CredentialId);
                if (nameExists)
                {
                    throw new ArgumentException($"Sheet config with name '{dto.Name}' already exists for this credential");
                }

                var spreadsheetExists = await _unitOfWork.SheetConfigs.IsSpreadsheetIdExistsAsync(dto.SpreadsheetId);
                if (spreadsheetExists)
                {
                    throw new ArgumentException($"Sheet config with spreadsheet ID '{dto.SpreadsheetId}' already exists");
                }

                // Test connection before creating
                var testResult = await TestSheetConnectionInternalAsync(dto.SpreadsheetId, credential.GetAbsolutePath(), dto.SheetName);
                if (!testResult.IsConnected)
                {
                    throw new ArgumentException($"Cannot connect to Google Sheet: {string.Join(", ", testResult.Errors)}");
                }

                // Create sheet config
                var sheetConfig = new GoogleSheetConfig
                {
                    Name = dto.Name.Trim(),
                    SpreadsheetId = dto.SpreadsheetId.Trim(),
                    SheetName = dto.SheetName?.Trim(),
                    Description = dto.Description?.Trim(),
                    CredentialId = dto.CredentialId,
                    HeaderRowIndex = dto.HeaderRowIndex,
                    DataStartRowIndex = dto.DataStartRowIndex,
                    IsActive = true
                };

                await _unitOfWork.SheetConfigs.AddAsync(sheetConfig);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created sheet config {SheetConfigId}: {SheetConfigName}",
                    sheetConfig.Id, sheetConfig.Name);

                return await MapToSheetConfigDtoAsync(sheetConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create sheet config: {SheetConfigName}", dto.Name);
                throw;
            }
        }

        public async Task<SheetConfigDto> UpdateSheetConfigAsync(int id, UpdateSheetConfigDto dto)
        {
            try
            {
                var sheetConfig = await _unitOfWork.SheetConfigs.GetConfigWithCredentialAsync(id);
                if (sheetConfig == null)
                {
                    throw new ArgumentException($"Sheet config with ID {id} not found");
                }

                // Check name uniqueness
                var nameExists = await _unitOfWork.SheetConfigs.IsNameExistsAsync(dto.Name, sheetConfig.CredentialId, id);
                if (nameExists)
                {
                    throw new ArgumentException($"Sheet config with name '{dto.Name}' already exists for this credential");
                }

                // Update properties
                sheetConfig.Name = dto.Name.Trim();
                sheetConfig.SheetName = dto.SheetName?.Trim();
                sheetConfig.Description = dto.Description?.Trim();
                sheetConfig.HeaderRowIndex = dto.HeaderRowIndex;
                sheetConfig.DataStartRowIndex = dto.DataStartRowIndex;
                sheetConfig.IsActive = dto.IsActive;
                sheetConfig.UpdatedAt = DateTime.Now;

                _unitOfWork.SheetConfigs.Update(sheetConfig);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated sheet config {SheetConfigId}: {SheetConfigName}", id, sheetConfig.Name);

                return await MapToSheetConfigDtoAsync(sheetConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update sheet config {SheetConfigId}", id);
                throw;
            }
        }

        public async Task DeleteSheetConfigAsync(int id)
        {
            try
            {
                var sheetConfig = await _unitOfWork.SheetConfigs.GetConfigWithCredentialAsync(id);
                if (sheetConfig == null)
                {
                    throw new ArgumentException($"Sheet config with ID {id} not found");
                }

                // Check if being used by races
                var raceCount = await _unitOfWork.SheetConfigs.GetRaceCountAsync(id);
                if (raceCount > 0)
                {
                    throw new InvalidOperationException($"Cannot delete sheet config. It is being used by {raceCount} race(s)");
                }

                _unitOfWork.SheetConfigs.Remove(sheetConfig);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted sheet config {SheetConfigId}: {SheetConfigName}", id, sheetConfig.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete sheet config {SheetConfigId}", id);
                throw;
            }
        }

        public async Task<SheetConfigDto?> GetSheetConfigAsync(int id)
        {
            var sheetConfig = await _unitOfWork.SheetConfigs.GetConfigWithCredentialAsync(id);
            return sheetConfig != null ? await MapToSheetConfigDtoAsync(sheetConfig) : null;
        }

        public async Task<IEnumerable<SheetConfigDto>> GetAllSheetConfigsAsync()
        {
            var sheetConfigs = await _unitOfWork.SheetConfigs.GetConfigsWithCredentialsAsync();
            return await MapToSheetConfigDtosAsync(sheetConfigs);
        }

        public async Task<IEnumerable<SheetConfigDto>> GetActiveSheetConfigsAsync()
        {
            var sheetConfigs = await _unitOfWork.SheetConfigs.GetActiveConfigsAsync();
            return await MapToSheetConfigDtosAsync(sheetConfigs);
        }

        public async Task<IEnumerable<SheetConfigDto>> GetConfigsByCredentialAsync(int credentialId)
        {
            var sheetConfigs = await _unitOfWork.SheetConfigs.GetConfigsByCredentialAsync(credentialId);
            return await MapToSheetConfigDtosAsync(sheetConfigs);
        }

        public async Task<SheetConfigTestResult> TestSheetConnectionAsync(int id)
        {
            try
            {
                var sheetConfig = await _unitOfWork.SheetConfigs.GetConfigWithCredentialAsync(id);
                if (sheetConfig == null)
                {
                    return new SheetConfigTestResult
                    {
                        SheetConfigId = id,
                        IsConnected = false,
                        Errors = new List<string> { "Sheet config not found" }
                    };
                }

                return await TestSheetConnectionInternalAsync(sheetConfig.SpreadsheetId,
                    sheetConfig.GetCredentialPath(), sheetConfig.SheetName, sheetConfig.Name, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test sheet config {SheetConfigId}", id);
                return new SheetConfigTestResult
                {
                    SheetConfigId = id,
                    IsConnected = false,
                    Errors = new List<string> { $"Test failed: {ex.Message}" }
                };
            }
        }

        public async Task<SheetMetadataDto> GetSheetMetadataAsync(int id)
        {
            var sheetConfig = await _unitOfWork.SheetConfigs.GetConfigWithCredentialAsync(id);
            if (sheetConfig == null)
            {
                throw new ArgumentException($"Sheet config with ID {id} not found");
            }

            return await _googleSheetsService.GetSheetMetadataAsync(sheetConfig.SpreadsheetId, sheetConfig.GetCredentialPath());
        }

        public async Task<IEnumerable<string>> GetSheetNamesAsync(int id)
        {
            var sheetConfig = await _unitOfWork.SheetConfigs.GetConfigWithCredentialAsync(id);
            if (sheetConfig == null)
            {
                throw new ArgumentException($"Sheet config with ID {id} not found");
            }

            return await _googleSheetsService.GetSheetNamesAsync(sheetConfig.SpreadsheetId, sheetConfig.GetCredentialPath());
        }

        public async Task<IEnumerable<SheetConfigDto>> SearchSheetConfigsAsync(string searchTerm)
        {
            var sheetConfigs = await _unitOfWork.SheetConfigs.SearchAsync(searchTerm);
            return await MapToSheetConfigDtosAsync(sheetConfigs);
        }

        public async Task<IEnumerable<SheetConfigSelectDto>> GetSheetConfigSelectListAsync()
        {
            var sheetConfigs = await _unitOfWork.SheetConfigs.GetActiveConfigsAsync();

            return sheetConfigs.Select(sc => new SheetConfigSelectDto
            {
                Id = sc.Id,
                Name = sc.Name,
                SpreadsheetId = sc.SpreadsheetId,
                CredentialName = sc.Credential.Name,
                IsActive = sc.IsActive,
                LastSyncRowIndex = sc.LastSyncRowIndex,
                RaceCount = sc.Races.Count
            }).ToList();
        }

        public async Task<IEnumerable<SheetConfigSelectDto>> GetSheetConfigSelectByCredentialAsync(int credentialId)
        {
            var sheetConfigs = await _unitOfWork.SheetConfigs.GetConfigsByCredentialAsync(credentialId);

            return sheetConfigs.Where(sc => sc.IsActive).Select(sc => new SheetConfigSelectDto
            {
                Id = sc.Id,
                Name = sc.Name,
                SpreadsheetId = sc.SpreadsheetId,
                CredentialName = sc.Credential.Name,
                IsActive = sc.IsActive,
                LastSyncRowIndex = sc.LastSyncRowIndex,
                RaceCount = sc.Races.Count
            }).ToList();
        }

        public async Task UpdateLastSyncRowAsync(int configId, int rowIndex)
        {
            await _unitOfWork.SheetConfigs.UpdateLastSyncRowAsync(configId, rowIndex);
            _logger.LogDebug("Updated last sync row for config {ConfigId} to row {RowIndex}", configId, rowIndex);
        }

        public async Task<int> GetNextSyncRowAsync(int configId)
        {
            var sheetConfig = await _unitOfWork.SheetConfigs.GetByIdAsync(configId);
            if (sheetConfig == null)
            {
                throw new ArgumentException($"Sheet config with ID {configId} not found");
            }

            return (sheetConfig.LastSyncRowIndex ?? sheetConfig.DataStartRowIndex - 1) + 1;
        }

        public async Task<BulkOperationResult> BulkUpdateStatusAsync(BulkSheetConfigOperation operation)
        {
            var result = new BulkOperationResult
            {
                TotalItems = operation.SheetConfigIds.Count
            };

            foreach (var configId in operation.SheetConfigIds)
            {
                try
                {
                    var sheetConfig = await _unitOfWork.SheetConfigs.GetByIdAsync(configId);
                    if (sheetConfig == null)
                    {
                        result.FailedItems++;
                        result.ItemResults[configId] = "Sheet config not found";
                        continue;
                    }

                    switch (operation.Operation.ToLower())
                    {
                        case "activate":
                            sheetConfig.IsActive = true;
                            break;
                        case "deactivate":
                            sheetConfig.IsActive = false;
                            break;
                        case "test":
                            var testResult = await TestSheetConnectionAsync(configId);
                            result.ItemResults[configId] = testResult.GetStatusMessage();
                            result.SuccessfulItems++;
                            continue;
                        default:
                            result.FailedItems++;
                            result.ItemResults[configId] = $"Unknown operation: {operation.Operation}";
                            continue;
                    }

                    _unitOfWork.SheetConfigs.Update(sheetConfig);
                    result.SuccessfulItems++;
                    result.ItemResults[configId] = $"Successfully {operation.Operation}d";
                }
                catch (Exception ex)
                {
                    result.FailedItems++;
                    result.ItemResults[configId] = ex.Message;
                    _logger.LogError(ex, "Bulk operation failed for sheet config {SheetConfigId}", configId);
                }
            }
            if (result.SuccessfulItems > 0 && operation.Operation.ToLower() != "test")
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return result;
        }

        // Helper methods
        private async Task<SheetConfigTestResult> TestSheetConnectionInternalAsync(
            string spreadsheetId, string credentialPath, string? sheetName = null,
            string configName = "", int configId = 0)
        {
            var result = new SheetConfigTestResult
            {
                SheetConfigId = configId,
                SheetConfigName = configName
            };

            try
            {
                // Test basic connection
                result.IsConnected = await _googleSheetsService.TestConnectionAsync(spreadsheetId, credentialPath);

                if (!result.IsConnected)
                {
                    result.Errors.Add("Cannot connect to Google Sheets");
                    return result;
                }

                // Try to get metadata
                try
                {
                    result.Metadata = await _googleSheetsService.GetSheetMetadataAsync(spreadsheetId, credentialPath);

                    // Verify specific sheet exists if specified
                    if (!string.IsNullOrEmpty(sheetName) && result.Metadata.Sheets.Any())
                    {
                        var sheetExists = result.Metadata.Sheets.Any(s =>
                            s.Title.Equals(sheetName, StringComparison.OrdinalIgnoreCase));

                        if (!sheetExists)
                        {
                            result.Errors.Add($"Sheet '{sheetName}' not found in spreadsheet");
                            result.CanReadData = false;
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Cannot get sheet metadata: {ex.Message}");
                }

                // Try to read data
                try
                {
                    result.TotalRows = await _googleSheetsService.GetTotalRowsAsync(spreadsheetId, "A:A", credentialPath);
                    result.DataRows = Math.Max(0, result.TotalRows.Value - 1); // Exclude header
                    result.CanReadData = true;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Cannot read data: {ex.Message}");
                    result.CanReadData = false;
                }

                _logger.LogInformation("Tested sheet connection for {SpreadsheetId}: Connected={IsConnected}, CanRead={CanRead}",
                    spreadsheetId, result.IsConnected, result.CanReadData);

            }
            catch (Exception ex)
            {
                result.IsConnected = false;
                result.CanReadData = false;
                result.Errors.Add($"Connection test failed: {ex.Message}");
                _logger.LogError(ex, "Sheet connection test failed for {SpreadsheetId}", spreadsheetId);
            }

            return result;
        }

        private async Task<SheetConfigDto> MapToSheetConfigDtoAsync(GoogleSheetConfig sheetConfig)
        {
            var raceCount = await _unitOfWork.SheetConfigs.GetRaceCountAsync(sheetConfig.Id);

            return new SheetConfigDto
            {
                Id = sheetConfig.Id,
                Name = sheetConfig.Name,
                SpreadsheetId = sheetConfig.SpreadsheetId,
                SheetName = sheetConfig.SheetName,
                Description = sheetConfig.Description,
                CredentialId = sheetConfig.CredentialId,
                CredentialName = sheetConfig.Credential?.Name ?? "Unknown",
                HeaderRowIndex = sheetConfig.HeaderRowIndex,
                DataStartRowIndex = sheetConfig.DataStartRowIndex,
                LastSyncRowIndex = sheetConfig.LastSyncRowIndex,
                IsActive = sheetConfig.IsActive,
                CreatedAt = sheetConfig.CreatedAt,
                UpdatedAt = sheetConfig.UpdatedAt,
                RaceCount = raceCount,
                SheetUrl = sheetConfig.GetSheetUrl(),
                DisplayName = sheetConfig.GetDisplayName()
            };
        }

        private async Task<IEnumerable<SheetConfigDto>> MapToSheetConfigDtosAsync(IEnumerable<GoogleSheetConfig> sheetConfigs)
        {
            var dtos = new List<SheetConfigDto>();

            foreach (var sheetConfig in sheetConfigs)
            {
                dtos.Add(await MapToSheetConfigDtoAsync(sheetConfig));
            }

            return dtos;
        }
    }
}
