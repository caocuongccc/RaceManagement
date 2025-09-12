using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RaceManagement.Core.Entities;
using RaceManagement.Core.Helpers;
using RaceManagement.Core.Interfaces;
using RaceManagement.Core.Models;
using RaceManagement.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RaceManagement.Application.Services
{
    public class CredentialService : ICredentialService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGoogleSheetsService _googleSheetsService;
        private readonly ILogger<CredentialService> _logger;

        public CredentialService(
            IUnitOfWork unitOfWork,
            IGoogleSheetsService googleSheetsService,
            ILogger<CredentialService> logger)
        {
            _unitOfWork = unitOfWork;
            _googleSheetsService = googleSheetsService;
            _logger = logger;
        }

        public async Task<CredentialDto> CreateCredentialAsync(CreateCredentialDto dto)
        {
            try
            {
                // Validate file
                var fileValidation = FileUploadHelper.ValidateCredentialFile(dto.CredentialFile);
                if (!fileValidation.IsValid)
                {
                    throw new ArgumentException($"Invalid credential file: {string.Join(", ", fileValidation.Errors)}");
                }

                // Parse service account info from uploaded file
                var serviceAccountInfo = await ParseServiceAccountFromFileAsync(dto.CredentialFile);
                if (serviceAccountInfo == null)
                {
                    throw new ArgumentException("Cannot parse service account information from file");
                }

                // Check for duplicates
                var existingByName = await _unitOfWork.Credentials.IsNameExistsAsync(dto.Name);
                if (existingByName)
                {
                    throw new ArgumentException($"Credential with name '{dto.Name}' already exists");
                }

                var existingByEmail = await _unitOfWork.Credentials.IsEmailExistsAsync(serviceAccountInfo.ClientEmail);
                if (existingByEmail)
                {
                    throw new ArgumentException($"Credential with email '{serviceAccountInfo.ClientEmail}' already exists");
                }

                // Upload file
                var filePath = await UploadCredentialFileAsync(dto.CredentialFile, dto.Name);

                // Create credential entity
                var credential = new GoogleCredential
                {
                    Name = dto.Name.Trim(),
                    Description = dto.Description?.Trim(),
                    ServiceAccountEmail = serviceAccountInfo.ClientEmail,
                    CredentialFilePath = filePath,
                    CreatedBy = dto.CreatedBy?.Trim(),
                    IsActive = true
                };

                await _unitOfWork.Credentials.AddAsync(credential);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created credential {CredentialId}: {CredentialName}",
                    credential.Id, credential.Name);

                return await MapToCredentialDtoAsync(credential);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create credential: {CredentialName}", dto.Name);
                throw;
            }
        }

        public async Task<CredentialDto> UpdateCredentialAsync(int id, UpdateCredentialDto dto)
        {
            try
            {
                var credential = await _unitOfWork.Credentials.GetByIdAsync(id);
                if (credential == null)
                {
                    throw new ArgumentException($"Credential with ID {id} not found");
                }

                // Check name uniqueness (excluding current credential)
                var nameExists = await _unitOfWork.Credentials.IsNameExistsAsync(dto.Name, id);
                if (nameExists)
                {
                    throw new ArgumentException($"Credential with name '{dto.Name}' already exists");
                }

                // Update basic properties
                credential.Name = dto.Name.Trim();
                credential.Description = dto.Description?.Trim();
                credential.IsActive = dto.IsActive;
                credential.UpdatedAt = DateTime.Now;

                // Handle credential file update if provided
                if (dto.CredentialFile != null)
                {
                    // Validate new file
                    var fileValidation = FileUploadHelper.ValidateCredentialFile(dto.CredentialFile);
                    if (!fileValidation.IsValid)
                    {
                        throw new ArgumentException($"Invalid credential file: {string.Join(", ", fileValidation.Errors)}");
                    }

                    // Parse new service account info
                    var serviceAccountInfo = await ParseServiceAccountFromFileAsync(dto.CredentialFile);
                    if (serviceAccountInfo == null)
                    {
                        throw new ArgumentException("Cannot parse service account information from new file");
                    }

                    // Check email uniqueness (excluding current credential)
                    var emailExists = await _unitOfWork.Credentials.IsEmailExistsAsync(serviceAccountInfo.ClientEmail, id);
                    if (emailExists)
                    {
                        throw new ArgumentException($"Credential with email '{serviceAccountInfo.ClientEmail}' already exists");
                    }

                    // Delete old file and upload new one
                    await DeleteCredentialFileAsync(credential.CredentialFilePath);
                    var newFilePath = await UploadCredentialFileAsync(dto.CredentialFile, dto.Name);

                    credential.ServiceAccountEmail = serviceAccountInfo.ClientEmail;
                    credential.CredentialFilePath = newFilePath;

                    // Clear Google Sheets service cache to use new credentials
                    _googleSheetsService.ClearCredentialCache();
                }

                _unitOfWork.Credentials.Update(credential);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated credential {CredentialId}: {CredentialName}", id, credential.Name);

                return await MapToCredentialDtoAsync(credential);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update credential {CredentialId}", id);
                throw;
            }
        }

        public async Task DeleteCredentialAsync(int id)
        {
            try
            {
                var credential = await _unitOfWork.Credentials.GetWithSheetConfigsAsync(id);
                if (credential == null)
                {
                    throw new ArgumentException($"Credential with ID {id} not found");
                }

                // Check if credential is being used by active sheet configs
                var activeSheetConfigs = credential.SheetConfigs.Where(sc => sc.IsActive).ToList();
                if (activeSheetConfigs.Any())
                {
                    throw new InvalidOperationException(
                        $"Cannot delete credential. It is being used by {activeSheetConfigs.Count} active sheet config(s): {string.Join(", ", activeSheetConfigs.Select(sc => sc.Name))}");
                }

                // Delete associated sheet configs first
                foreach (var sheetConfig in credential.SheetConfigs)
                {
                    _unitOfWork.SheetConfigs.Remove(sheetConfig);
                }

                // Delete credential file
                await DeleteCredentialFileAsync(credential.CredentialFilePath);

                // Delete credential
                _unitOfWork.Credentials.Remove(credential);
                await _unitOfWork.SaveChangesAsync();

                // Clear cache
                _googleSheetsService.ClearCredentialCache();

                _logger.LogInformation("Deleted credential {CredentialId}: {CredentialName}", id, credential.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete credential {CredentialId}", id);
                throw;
            }
        }

        public async Task<CredentialDto?> GetCredentialAsync(int id)
        {
            var credential = await _unitOfWork.Credentials.GetWithSheetConfigsAsync(id);
            return credential != null ? await MapToCredentialDtoAsync(credential) : null;
        }

        public async Task<IEnumerable<CredentialDto>> GetAllCredentialsAsync()
        {
            var credentials = await _unitOfWork.Credentials.GetCredentialsWithSheetConfigsAsync();
            return await MapToCredentialDtosAsync(credentials);
        }

        public async Task<IEnumerable<CredentialDto>> GetActiveCredentialsAsync()
        {
            var credentials = await _unitOfWork.Credentials.GetActiveCredentialsAsync();
            return await MapToCredentialDtosAsync(credentials);
        }

        public async Task<CredentialTestResult> TestCredentialAsync(int id)
        {
            try
            {
                var credential = await _unitOfWork.Credentials.GetByIdAsync(id);
                if (credential == null)
                {
                    return new CredentialTestResult
                    {
                        CredentialId = id,
                        IsValid = false,
                        Errors = new List<string> { "Credential not found" }
                    };
                }

                var result = new CredentialTestResult
                {
                    CredentialId = id,
                    CredentialName = credential.Name,
                    FileExists = credential.FileExists()
                };

                if (!result.FileExists)
                {
                    result.Errors.Add($"Credential file not found: {credential.CredentialFilePath}");
                    return result;
                }

                // Validate credential file
                var validation = await credential.ValidateAsync();
                result.IsValid = validation.IsValid;
                result.Errors.AddRange(validation.Errors);

                if (validation.ServiceAccountInfo != null)
                {
                    result.ServiceAccountInfo = new ServiceAccountInfoDto
                    {
                        ClientEmail = validation.ServiceAccountInfo.ClientEmail,
                        ClientId = validation.ServiceAccountInfo.ClientId,
                        ProjectId = validation.ServiceAccountInfo.ProjectId,
                        PrivateKeyId = validation.ServiceAccountInfo.PrivateKeyId,
                        Type = validation.ServiceAccountInfo.Type
                    };
                }

                // Test authentication by creating a temporary sheet service
                if (result.IsValid)
                {
                    try
                    {
                        // Test with a dummy sheet ID - just to verify authentication works
                        var testSheetId = "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms"; // Google's test sheet
                        result.CanAuthenticate = await _googleSheetsService.TestConnectionAsync(testSheetId, credential.GetAbsolutePath());
                    }
                    catch (Exception ex)
                    {
                        result.CanAuthenticate = false;
                        result.Errors.Add($"Authentication test failed: {ex.Message}");
                    }
                }

                _logger.LogInformation("Tested credential {CredentialId}: Valid={IsValid}, CanAuth={CanAuth}",
                    id, result.IsValid, result.CanAuthenticate);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test credential {CredentialId}", id);
                return new CredentialTestResult
                {
                    CredentialId = id,
                    IsValid = false,
                    Errors = new List<string> { $"Test failed: {ex.Message}" }
                };
            }
        }

        public async Task<string> UploadCredentialFileAsync(IFormFile file, string credentialName)
        {
            try
            {
                var filePath = await FileUploadHelper.SaveCredentialFileAsync(file, credentialName);
                _logger.LogInformation("Uploaded credential file: {FilePath}", filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload credential file for {CredentialName}", credentialName);
                throw;
            }
        }

        public Task<bool> DeleteCredentialFileAsync(string filePath)
        {
            try
            {
                var absolutePath = Path.IsPathRooted(filePath)
                    ? filePath
                    : Path.Combine(Directory.GetCurrentDirectory(), filePath);

                if (File.Exists(absolutePath))
                {
                    File.Delete(absolutePath);
                    _logger.LogInformation("Deleted credential file: {FilePath}", filePath);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete credential file: {FilePath}", filePath);
                return Task.FromResult(false);
            }
        }

        public async Task<IEnumerable<CredentialDto>> SearchCredentialsAsync(string searchTerm)
        {
            var credentials = await _unitOfWork.Credentials.SearchAsync(searchTerm);
            return await MapToCredentialDtosAsync(credentials);
        }

        public async Task<IEnumerable<CredentialSelectDto>> GetCredentialSelectListAsync()
        {
            var credentials = await _unitOfWork.Credentials.GetActiveCredentialsAsync();

            return credentials.Select(c => new CredentialSelectDto
            {
                Id = c.Id,
                Name = c.Name,
                ServiceAccountEmail = c.ServiceAccountEmail,
                SheetConfigCount = c.SheetConfigs.Count,
                IsActive = c.IsActive,
                FileExists = c.FileExists()
            }).ToList();
        }

        public async Task<BulkOperationResult> BulkUpdateStatusAsync(BulkCredentialOperation operation)
        {
            var result = new BulkOperationResult
            {
                TotalItems = operation.CredentialIds.Count
            };

            foreach (var credentialId in operation.CredentialIds)
            {
                try
                {
                    var credential = await _unitOfWork.Credentials.GetByIdAsync(credentialId);
                    if (credential == null)
                    {
                        result.FailedItems++;
                        result.ItemResults[credentialId] = "Credential not found";
                        continue;
                    }

                    switch (operation.Operation.ToLower())
                    {
                        case "activate":
                            credential.IsActive = true;
                            break;
                        case "deactivate":
                            credential.IsActive = false;
                            break;
                        case "test":
                            var testResult = await TestCredentialAsync(credentialId);
                            result.ItemResults[credentialId] = testResult.GetStatusMessage();
                            result.SuccessfulItems++;
                            continue;
                        default:
                            result.FailedItems++;
                            result.ItemResults[credentialId] = $"Unknown operation: {operation.Operation}";
                            continue;
                    }

                    _unitOfWork.Credentials.Update(credential);
                    result.SuccessfulItems++;
                    result.ItemResults[credentialId] = $"Successfully {operation.Operation}d";
                }
                catch (Exception ex)
                {
                    result.FailedItems++;
                    result.ItemResults[credentialId] = ex.Message;
                    _logger.LogError(ex, "Bulk operation failed for credential {CredentialId}", credentialId);
                }
            }

            if (result.SuccessfulItems > 0 && operation.Operation.ToLower() != "test")
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return result;
        }

        // Helper methods
        private async Task<ServiceAccountInfo?> ParseServiceAccountFromFileAsync(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                var jsonDoc = await JsonDocument.ParseAsync(stream);
                var root = jsonDoc.RootElement;

                return new ServiceAccountInfo
                {
                    ClientEmail = root.GetProperty("client_email").GetString() ?? string.Empty,
                    ClientId = root.GetProperty("client_id").GetString() ?? string.Empty,
                    ProjectId = root.GetProperty("project_id").GetString() ?? string.Empty,
                    PrivateKeyId = root.GetProperty("private_key_id").GetString() ?? string.Empty,
                    Type = root.GetProperty("type").GetString() ?? string.Empty
                };
            }
            catch
            {
                return null;
            }
        }

        private async Task<CredentialDto> MapToCredentialDtoAsync(GoogleCredential credential)
        {
            var sheetConfigCount = await _unitOfWork.Credentials.GetSheetConfigCountAsync(credential.Id);

            return new CredentialDto
            {
                Id = credential.Id,
                Name = credential.Name,
                Description = credential.Description,
                ServiceAccountEmail = credential.ServiceAccountEmail,
                CredentialFilePath = credential.CredentialFilePath,
                IsActive = credential.IsActive,
                CreatedBy = credential.CreatedBy,
                CreatedAt = credential.CreatedAt,
                UpdatedAt = credential.UpdatedAt,
                SheetConfigCount = sheetConfigCount,
                FileExists = credential.FileExists()
            };
        }

        private async Task<IEnumerable<CredentialDto>> MapToCredentialDtosAsync(IEnumerable<GoogleCredential> credentials)
        {
            var dtos = new List<CredentialDto>();

            foreach (var credential in credentials)
            {
                dtos.Add(await MapToCredentialDtoAsync(credential));
            }

            return dtos;
        }

        public async Task<CredentialValidationResult> ValidateCredentialFileAsync(IFormFile file)
        {
            var result = new CredentialValidationResult();

            // Validate file format
            var fileValidation = FileUploadHelper.ValidateCredentialFile(file);
            if (!fileValidation.IsValid)
            {
                result.Errors.AddRange(fileValidation.Errors);
                return result;
            }

            // Parse and validate JSON content
            var serviceAccountInfo = await ParseServiceAccountFromFileAsync(file);
            if (serviceAccountInfo == null)
            {
                result.AddError("Cannot parse service account JSON");
                return result;
            }

            // Validate required fields
            if (string.IsNullOrEmpty(serviceAccountInfo.ClientEmail))
                result.AddError("Missing client_email in JSON");

            if (string.IsNullOrEmpty(serviceAccountInfo.ProjectId))
                result.AddError("Missing project_id in JSON");

            if (serviceAccountInfo.Type != "service_account")
                result.AddError("Invalid type, must be 'service_account'");

            result.ServiceAccountInfo = serviceAccountInfo;

            return result;
        }
        

    }
}
