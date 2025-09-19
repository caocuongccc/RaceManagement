using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RaceManagement.Core.Interfaces;
using RaceManagement.Shared.DTOs;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RaceManagement.Infrastructure.Services
{
    public class GoogleSheetsService : IGoogleSheetsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleSheetsService> _logger;
        private readonly Dictionary<string, SheetsService> _sheetsServices;
        private readonly object _lockObject = new object();

        public GoogleSheetsService(IConfiguration configuration, ILogger<GoogleSheetsService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _sheetsServices = new Dictionary<string, SheetsService>();
        }

        private SheetsService GetSheetsService(string? credentialPath = null)
        {
            lock (_lockObject)
            {
                // Use specific credential path or default
                var keyPath = credentialPath ?? _configuration["GoogleSheets:ServiceAccountKeyPath"];

                if (string.IsNullOrEmpty(keyPath))
                {
                    throw new InvalidOperationException("Google Sheets credential path is not configured");
                }

                // Convert relative path to absolute
                if (!Path.IsPathRooted(keyPath))
                {
                    keyPath = Path.Combine(Directory.GetCurrentDirectory(), keyPath);
                }

                if (!File.Exists(keyPath))
                {
                    throw new FileNotFoundException($"Google Service Account file not found: {keyPath}");
                }

                // Cache SheetsService instances per credential file
                if (!_sheetsServices.ContainsKey(keyPath))
                {
                    try
                    {
                        var credential = GoogleCredential.FromFile(keyPath)
                            .CreateScoped(SheetsService.Scope.Spreadsheets);

                        var service = new SheetsService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = credential,
                            ApplicationName = _configuration["GoogleSheets:ApplicationName"] ?? "Race Management System"
                        });

                        _sheetsServices[keyPath] = service;
                        _logger.LogInformation("Created Google Sheets Service for credential: {CredentialPath}",
                            Path.GetFileName(keyPath));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create Google Sheets Service for credential: {CredentialPath}",
                            keyPath);
                        throw;
                    }
                }

                return _sheetsServices[keyPath];
            }
        }

        public async Task<IEnumerable<SheetRegistrationDto>> ReadNewRegistrationsAsync(
            string sheetId, int fromRowIndex, string? credentialPath = null)
        {
            try
            {
                var sheetsService = GetSheetsService(credentialPath);
                var range = $"A{fromRowIndex + 1}:Z1000"; // Read generous range

                _logger.LogInformation("Reading registrations from sheet {SheetId}, range {Range}", sheetId, range);

                var request = sheetsService.Spreadsheets.Values.Get(sheetId, range);
                var response = await request.ExecuteAsync();

                var registrations = new List<SheetRegistrationDto>();

                if (response.Values != null && response.Values.Count > 0)
                {
                    _logger.LogInformation("Found {Count} rows in sheet {SheetId} from row {FromRow}",
                        response.Values.Count, sheetId, fromRowIndex + 1);

                    for (int i = 0; i < response.Values.Count; i++)
                    {
                        var row = response.Values[i];
                        var currentRowIndex = fromRowIndex + i + 1;

                        // Skip empty rows
                        if (IsEmptyRow(row))
                        {
                            _logger.LogDebug("Skipping empty row {RowIndex}", currentRowIndex);
                            continue;
                        }

                        try
                        {
                            var registration = MapRowToRegistration(row, currentRowIndex);

                            // Basic validation
                            if (IsValidRegistration(registration))
                            {
                                registrations.Add(registration);
                                _logger.LogDebug("Mapped registration for {Email} at row {Row}",
                                    registration.Email, currentRowIndex);
                            }
                            else
                            {
                                _logger.LogWarning("Invalid registration data at row {RowIndex}: {Email}",
                                    currentRowIndex, registration.Email);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to parse row {RowIndex} in sheet {SheetId}",
                                currentRowIndex, sheetId);
                            // Continue processing other rows
                        }
                    }
                }

                _logger.LogInformation("Successfully processed {Count} valid registrations from sheet {SheetId}",
                    registrations.Count, sheetId);

                return registrations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading registrations from sheet {SheetId}", sheetId);
                throw;
            }
        }

        // Map theo cấu trúc thực tế của file Excel
        // A=Timestamp, B=Email, D=FullName, E=BibName, F=DOB, G=Phone, H=Distance, I=Gender, L=ShirtCategory, M=ShirtSize, N=ShirtType
        private SheetRegistrationDto MapRowToRegistration(IList<object> row, int rowIndex)
        {
            var registration = new SheetRegistrationDto
            {
                RowIndex = rowIndex,
                Timestamp = ParseDateTime(GetCellValue(row, 0)),      // A - Timestamp
                Email = GetCellValue(row, 1).ToLower().Trim(),        // B - Email (normalize)
                FullName = GetCellValue(row, 3).Trim(),               // D - Tên VĐV
                BibName = GetCellValue(row, 4).Trim(),                // E - Tên trên BIB
                RawBirthInput = GetCellValue(row, 5),                 // F - Ngày sinh (raw)
                Phone = NormalizePhoneNumber(GetCellValue(row, 6)),   // G - SĐT (normalize)
                Distance = GetCellValue(row, 7).Trim(),               // H - Cự ly
                Gender = NormalizeGender(GetCellValue(row, 8)),       // I - Giới tính
                ShirtCategory = NormalizeShirtCategory(GetCellValue(row, 11)), // L - Loại áo
                ShirtSize = NormalizeShirtSize(GetCellValue(row, 13)), // M - Size áo
                ShirtType = NormalizeShirtType(GetCellValue(row, 12)), // N - Kiểu áo
            };

            // Parse birth date with advanced logic
            registration.DateOfBirth = ParseDateOfBirth(registration.RawBirthInput);
            registration.BirthYear = registration.DateOfBirth?.Year;

            // If BibName is empty, use FullName
            if (string.IsNullOrWhiteSpace(registration.BibName))
            {
                registration.BibName = registration.FullName;
            }

            return registration;
        }

        // Advanced Vietnamese date parsing
        private DateTime? ParseDateOfBirth(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var cleanInput = input.Trim();
            _logger.LogDebug("Parsing birth date: '{Input}'", cleanInput);

            // Common Vietnamese date formats
            var formats = new[]
            {
            // Standard formats
            "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy",
            "yyyy/MM/dd", "yyyy-MM-dd", "yyyy/M/d", "yyyy-M-d",
            "dd/MM/yy", "d/M/yy", "dd-MM-yy", "d-M-yy",
            
            // US formats (just in case)
            "MM/dd/yyyy", "M/d/yyyy", "MM-dd-yyyy", "M-d-yyyy",
            
            // Vietnamese text formats
            "dd 'tháng' MM 'năm' yyyy",
            "d 'tháng' M 'năm' yyyy",
            "dd/MM/yyyy",
        };

            // Try exact format parsing first
            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(cleanInput, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime exactResult))
                {
                    // Handle 2-digit years
                    if (exactResult.Year < 100)
                    {
                        exactResult = exactResult.AddYears(exactResult.Year < 50 ? 2000 : 1900);
                    }

                    _logger.LogDebug("Parsed date '{Input}' as {Result} using format {Format}",
                        input, exactResult, format);
                    return exactResult;
                }
            }

            // Try regex patterns for more flexible parsing
            var datePatterns = new[]
            {
            @"(\d{1,2})[\/\-](\d{1,2})[\/\-](\d{2,4})", // dd/mm/yyyy or dd-mm-yyyy
            @"(\d{4})[\/\-](\d{1,2})[\/\-](\d{1,2})",   // yyyy/mm/dd or yyyy-mm-dd
        };

            foreach (var pattern in datePatterns)
            {
                var match = Regex.Match(cleanInput, pattern);
                if (match.Success)
                {
                    try
                    {
                        if (pattern.Contains(@"(\d{4})")) // yyyy/mm/dd pattern
                        {
                            var year = int.Parse(match.Groups[1].Value);
                            var month = int.Parse(match.Groups[2].Value);
                            var day = int.Parse(match.Groups[3].Value);

                            var result = new DateTime(year, month, day);
                            _logger.LogDebug("Parsed date '{Input}' as {Result} using regex", input, result);
                            return result;
                        }
                        else // dd/mm/yyyy pattern
                        {
                            var day = int.Parse(match.Groups[1].Value);
                            var month = int.Parse(match.Groups[2].Value);
                            var year = int.Parse(match.Groups[3].Value);

                            // Handle 2-digit years
                            if (year < 100)
                            {
                                year += year < 50 ? 2000 : 1900;
                            }

                            var result = new DateTime(year, month, day);
                            _logger.LogDebug("Parsed date '{Input}' as {Result} using regex", input, result);
                            return result;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Failed to parse date components from '{Input}'", input);
                    }
                }
            }

            // Try general parsing as last resort
            if (DateTime.TryParse(cleanInput, out DateTime generalResult))
            {
                _logger.LogDebug("Parsed date '{Input}' as {Result} using general parsing", input, generalResult);
                return generalResult;
            }

            _logger.LogWarning("Could not parse birth date: '{Input}'", input);
            return null;
        }

        // Normalization methods for data consistency
        private string NormalizePhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            // Remove all non-digit characters
            var digits = Regex.Replace(phone, @"[^\d]", "");

            // Handle Vietnamese phone formats
            if (digits.StartsWith("84"))
            {
                // Convert +84 to 0
                digits = "0" + digits.Substring(2);
            }
            else if (!digits.StartsWith("0") && digits.Length >= 9)
            {
                // Add 0 prefix for mobile numbers
                digits = "0" + digits;
            }

            return digits;
        }

        private string NormalizeGender(string gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
                return string.Empty;

            var normalized = gender.Trim().ToLower();

            // Vietnamese gender normalization
            if (normalized.Contains("nam") || normalized.Contains("male") || normalized == "m")
                return "M";

            if (normalized.Contains("nữ") || normalized.Contains("nu") ||
                normalized.Contains("female") || normalized == "f")
                return "F";

            return gender.Trim(); // Return original if can't normalize
        }

        private string NormalizeShirtCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return string.Empty;

            var normalized = category.Trim().ToLower();

            // Vietnamese shirt category normalization
            if (normalized.Contains("nam") || normalized.Contains("male"))
                return "Nam";

            if (normalized.Contains("nữ") || normalized.Contains("nu") || normalized.Contains("female"))
                return "Nữ";

            if (normalized.Contains("trẻ") || normalized.Contains("em") ||
                normalized.Contains("kid") || normalized.Contains("child"))
                return "Trẻ em";

            return category.Trim();
        }

        private string NormalizeShirtSize(string size)
        {
            if (string.IsNullOrWhiteSpace(size))
                return string.Empty;

            var normalized = size.Trim().ToUpper();

            // Handle kid sizes
            if (normalized.Contains("KID"))
                return normalized;

            // Standard adult sizes
            var standardSizes = new[] { "XS", "S", "M", "L", "XL", "XXL", "XXXL" };

            foreach (var standardSize in standardSizes)
            {
                if (normalized.Contains(standardSize))
                    return standardSize;
            }

            return size.Trim();
        }

        private string NormalizeShirtType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return "T-Shirt"; // Default

            var normalized = type.Trim().ToLower();

            if (normalized.Contains("singlet") || normalized.Contains("tank"))
                return "Singlet";

            if (normalized.Contains("t-shirt") || normalized.Contains("tshirt") ||
                normalized.Contains("áo thun"))
                return "T-Shirt";

            return type.Trim();
        }

        private bool IsValidRegistration(SheetRegistrationDto registration)
        {
            // Basic validation rules
            return !string.IsNullOrWhiteSpace(registration.Email) &&
                   !string.IsNullOrWhiteSpace(registration.FullName) &&
                   !string.IsNullOrWhiteSpace(registration.Distance) &&
                   IsValidEmail(registration.Email);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TestConnectionAsync(string sheetId, string? credentialPath = null)
        {
            try
            {
                var sheetsService = GetSheetsService(credentialPath);
                var request = sheetsService.Spreadsheets.Values.Get(sheetId, "A1:A1");
                await request.ExecuteAsync();

                _logger.LogInformation("Successfully connected to sheet {SheetId} with credential {CredentialPath}",
                    sheetId, credentialPath ?? "default");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to sheet {SheetId} with credential {CredentialPath}",
                    sheetId, credentialPath ?? "default");
                return false;
            }
        }

        public async Task<int> GetTotalRowsAsync(string sheetId, string range = "A:A", string? credentialPath = null)
        {
            try
            {
                var sheetsService = GetSheetsService(credentialPath);
                var request = sheetsService.Spreadsheets.Values.Get(sheetId, range);
                var response = await request.ExecuteAsync();

                var totalRows = response.Values?.Count ?? 0;
                _logger.LogInformation("Sheet {SheetId} has {TotalRows} rows in range {Range}",
                    sheetId, totalRows, range);

                return totalRows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total rows from sheet {SheetId}", sheetId);
                return 0;
            }
        }

        public async Task<SheetMetadataDto> GetSheetMetadataAsync(string sheetId, string? credentialPath = null)
        {
            try
            {
                var sheetsService = GetSheetsService(credentialPath);
                var request = sheetsService.Spreadsheets.Get(sheetId);
                var spreadsheet = await request.ExecuteAsync();

                var metadata = new SheetMetadataDto
                {
                    SpreadsheetId = spreadsheet.SpreadsheetId ?? sheetId,
                    Title = spreadsheet.Properties?.Title ?? "Untitled",
                    SheetCount = spreadsheet.Sheets?.Count ?? 0,
                    Owner = "Unknown", // Google Sheets API doesn't provide owner in basic request
                    LastModified = DateTime.Now // Placeholder - would need Drive API for actual date
                };

                // Map sheet information
                if (spreadsheet.Sheets != null)
                {
                    metadata.Sheets = spreadsheet.Sheets.Select(sheet => new SheetInfoDto
                    {
                        SheetId = sheet.Properties?.SheetId ?? 0,
                        Title = sheet.Properties?.Title ?? "Untitled",
                        RowCount = sheet.Properties?.GridProperties?.RowCount ?? 0,
                        ColumnCount = sheet.Properties?.GridProperties?.ColumnCount ?? 0,
                        SheetType = sheet.Properties?.SheetType ?? "GRID",
                        IsHidden = sheet.Properties?.Hidden ?? false
                    }).ToList();
                }

                _logger.LogInformation("Retrieved metadata for spreadsheet {SpreadsheetId}: {Title} with {SheetCount} sheets",
                    metadata.SpreadsheetId, metadata.Title, metadata.SheetCount);

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metadata for sheet {SheetId}", sheetId);
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetSheetNamesAsync(string spreadsheetId, string? credentialPath = null)
        {
            try
            {
                var metadata = await GetSheetMetadataAsync(spreadsheetId, credentialPath);
                return metadata.SheetNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sheet names for {SpreadsheetId}", spreadsheetId);
                return new List<string>();
            }
        }

        public void ClearCredentialCache()
        {
            lock (_lockObject)
            {
                foreach (var service in _sheetsServices.Values)
                {
                    service?.Dispose();
                }
                _sheetsServices.Clear();
                _logger.LogInformation("Cleared Google Sheets credential cache");
            }
        }

        // Implementation for other interface methods
        public async Task<string> CreatePaymentTrackingSheetAsync(string sourceSheetId, string raceName, string? credentialPath = null)
        {
            try
            {
                var sheetsService = GetSheetsService(credentialPath);

                // Get source spreadsheet
                var spreadsheetRequest = sheetsService.Spreadsheets.Get(sourceSheetId);
                var spreadsheet = await spreadsheetRequest.ExecuteAsync();

                if (spreadsheet.Sheets == null || !spreadsheet.Sheets.Any())
                {
                    throw new InvalidOperationException("Source spreadsheet has no sheets");
                }

                var sourceSheet = spreadsheet.Sheets.First();

                // Create copy of the first sheet
                var copyRequest = new CopySheetToAnotherSpreadsheetRequest
                {
                    DestinationSpreadsheetId = sourceSheetId
                };

                var copyResponse = await sheetsService.Spreadsheets.Sheets
                    .CopyTo(copyRequest, sourceSheetId, sourceSheet.Properties.SheetId.Value)
                    .ExecuteAsync();

                // Rename the copied sheet
                var newSheetName = $"{raceName} - Payment Tracking";
                var renameRequest = new BatchUpdateSpreadsheetRequest
                {
                    Requests = new List<Request>
                {
                    new Request
                    {
                        UpdateSheetProperties = new UpdateSheetPropertiesRequest
                        {
                            Properties = new SheetProperties
                            {
                                SheetId = copyResponse.SheetId,
                                Title = newSheetName
                            },
                            Fields = "title"
                        }
                    }
                }
                };

                await sheetsService.Spreadsheets.BatchUpdate(renameRequest, sourceSheetId).ExecuteAsync();

                // Add payment status column header
                await AddPaymentColumnAsync(sheetsService, sourceSheetId, copyResponse.SheetId.Value);

                _logger.LogInformation("Created payment tracking sheet '{SheetName}' for race {RaceName}",
                    newSheetName, raceName);

                return sourceSheetId; // Return the same spreadsheet ID with new sheet
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment tracking sheet for race {RaceName}", raceName);
                throw;
            }
        }

        public async Task MarkPaymentStatusAsync(string sheetId, int rowIndex, bool isPaid, string? credentialPath = null)
        {
            try
            {
                var sheetsService = GetSheetsService(credentialPath);

                // Find the payment status column (usually the last column with data)
                var headerRange = "A1:Z1";
                var headerRequest = sheetsService.Spreadsheets.Values.Get(sheetId, headerRange);
                var headerResponse = await headerRequest.ExecuteAsync();

                var paymentColumnIndex = FindPaymentColumnIndex(headerResponse.Values?.FirstOrDefault());

                if (paymentColumnIndex == -1)
                {
                    _logger.LogWarning("Payment column not found in sheet {SheetId}", sheetId);
                    return;
                }

                var columnLetter = GetColumnLetter(paymentColumnIndex);
                var cellRange = $"{columnLetter}{rowIndex}";

                var valueRange = new ValueRange
                {
                    Range = cellRange,
                    Values = new List<IList<object>>
                {
                    new List<object> { isPaid ? "X" : "" }
                }
                };

                var updateRequest = sheetsService.Spreadsheets.Values.Update(valueRange, sheetId, cellRange);
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

                await updateRequest.ExecuteAsync();

                _logger.LogInformation("Marked payment status {Status} for row {RowIndex} in sheet {SheetId}",
                    isPaid ? "PAID" : "UNPAID", rowIndex, sheetId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking payment status for row {RowIndex} in sheet {SheetId}",
                    rowIndex, sheetId);
                throw;
            }
        }

        // Helper methods
        private bool IsEmptyRow(IList<object> row)
        {
            return row == null || row.Count == 0 ||
                   row.All(cell => string.IsNullOrWhiteSpace(cell?.ToString()));
        }

        private string GetCellValue(IList<object> row, int columnIndex)
        {
            if (row == null || columnIndex >= row.Count)
                return string.Empty;

            return row[columnIndex]?.ToString()?.Trim() ?? string.Empty;
        }

        private DateTime ParseDateTime(string value)
        {
            if (DateTime.TryParse(value, out DateTime result))
                return result;

            return DateTime.Now;
        }

        private async Task AddPaymentColumnAsync(SheetsService sheetsService, string spreadsheetId, int sheetId)
        {
            var requests = new List<Request>
        {
            new Request
            {
                UpdateCells = new UpdateCellsRequest
                {
                    Range = new GridRange
                    {
                        SheetId = sheetId,
                        StartRowIndex = 0,
                        EndRowIndex = 1,
                        StartColumnIndex = 20, // Column U
                        EndColumnIndex = 21
                    },
                    Rows = new List<RowData>
                    {
                        new RowData
                        {
                            Values = new List<CellData>
                            {
                                new CellData
                                {
                                    UserEnteredValue = new ExtendedValue { StringValue = "Thanh toán" },
                                    UserEnteredFormat = new CellFormat
                                    {
                                        TextFormat = new TextFormat { Bold = true }
                                    }
                                }
                            }
                        }
                    },
                    Fields = "userEnteredValue,userEnteredFormat.textFormat.bold"
                }
            }
        };

            var batchRequest = new BatchUpdateSpreadsheetRequest { Requests = requests };
            await sheetsService.Spreadsheets.BatchUpdate(batchRequest, spreadsheetId).ExecuteAsync();
        }

        private int FindPaymentColumnIndex(IList<object>? headerRow)
        {
            if (headerRow == null) return 20; // Default to column U

            for (int i = 0; i < headerRow.Count; i++)
            {
                var header = headerRow[i]?.ToString()?.ToLower() ?? string.Empty;
                if (header.Contains("thanh toán") || header.Contains("payment") ||
                    header.Contains("paid") || header.Contains("status"))
                {
                    return i;
                }
            }

            return 20; // Default to column U if not found
        }

        private string GetColumnLetter(int columnIndex)
        {
            string columnLetter = "";
            while (columnIndex >= 0)
            {
                columnLetter = (char)('A' + columnIndex % 26) + columnLetter;
                columnIndex = columnIndex / 26 - 1;
            }
            return columnLetter;
        }

        public void Dispose()
        {
            ClearCredentialCache();
        }
    }
}
