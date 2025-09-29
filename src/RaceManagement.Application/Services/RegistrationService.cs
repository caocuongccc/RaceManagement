using Hangfire;
using Microsoft.Extensions.Logging;
using RaceManagement.Application.Jobs;
using RaceManagement.Core.Entities;
using RaceManagement.Shared.Enums;
using RaceManagement.Core.Interfaces;
using RaceManagement.Shared.DTOs;
using RaceManagement.Application.Helpers;

namespace RaceManagement.Application.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGoogleSheetsService _googleSheetsService;
        private readonly ILogger<RegistrationService> _logger;
        private readonly IQRCodeService _qRCodeService;
        IRegistrationRepository _registrationRepository;

        public RegistrationService(
            IUnitOfWork unitOfWork,
            IGoogleSheetsService googleSheetsService,
            IRegistrationRepository registrationRepository,
            ILogger<RegistrationService> logger,
            IQRCodeService qRCodeService)
        {
            _unitOfWork = unitOfWork;
            _googleSheetsService = googleSheetsService;
            _registrationRepository = registrationRepository;
            _logger = logger;
            _qRCodeService = qRCodeService; 
        }
        public async Task<List<RegistrationDto>> GetAllAsync()
        {
            var regs = await _registrationRepository.GetAllWithDetailsAsync();
            return regs.Select(r => new RegistrationDto
            {
                RaceId = r.RaceId,
                RaceName = r.Race.Name,
                DistanceId = r.DistanceId,
                Distance = r.Distance.Distance,   // vì RaceDistance có property Distance
                Price = r.Distance.Price,
                FullName = r.FullName,
                BibName = r.BibName,
                Email = r.Email,
                Phone = r.Phone,
                BirthYear = r.BirthYear,
                DateOfBirth = r.DateOfBirth,
                RawBirthInput = r.RawBirthInput,
                Gender = r.Gender.HasValue ? r.Gender.Value.ToString() : null,
                ShirtCategory = r.ShirtCategory,
                ShirtSize = r.ShirtSize,
                ShirtType = r.ShirtType,
                EmergencyContact = r.EmergencyContact,
                RegistrationTime = r.RegistrationTime,
                PaymentStatus = r.PaymentStatus.ToString(),
                BibNumber = r.BibNumber,
                BibSentAt = r.BibSentAt,
                TransactionReference = r.TransactionReference
            }).ToList();
        }
        public async Task<SyncResultDto> SyncRegistrationsFromSheetAsync(int raceId)
        {
            var result = new SyncResultDto { RaceId = raceId };
            var newRegistrations = new List<Registration>();

            try
            {
                var race = await _unitOfWork.Races.GetRaceWithDistancesAsync(raceId);
                if (race == null)
                {
                    result.Error = $"Race {raceId} not found";
                    return result;
                }
                // Determine sheet access method
                string spreadsheetId;
                string? credentialPath = null;
                int fromRowIndex;

                if (race.HasSheetConfig)
                {
                    // NEW SYSTEM - Use sheet config
                    spreadsheetId = race.SheetConfig!.SpreadsheetId;
                    credentialPath = race.SheetConfig.GetCredentialPath();
                    fromRowIndex = race.SheetConfig.LastSyncRowIndex ?? (race.SheetConfig.DataStartRowIndex - 1);

                    _logger.LogInformation("Using sheet config {ConfigId}: {ConfigName} for race {RaceId}",
                        race.SheetConfig.Id, race.SheetConfig.Name, raceId);
                }
                else
                {
                    // LEGACY SYSTEM - Use direct sheet ID (backward compatibility)
                    spreadsheetId = race.SheetId ?? throw new InvalidOperationException("No sheet configuration found for this race");
                    fromRowIndex = await _unitOfWork.Registrations.GetLastSheetRowIndexAsync(raceId);

                    _logger.LogWarning("Using legacy sheet ID {SheetId} for race {RaceId}. Consider updating to sheet config system.",
                        spreadsheetId, raceId);
                }
                // Get last processed row
                var lastRowIndex = await _unitOfWork.Registrations.GetLastSheetRowIndexAsync(raceId);

                // Read new registrations from Google Sheet
                var sheetRegistrations = await _googleSheetsService
                    .ReadNewRegistrationsAsync(race.SheetConfig!.SpreadsheetId, lastRowIndex);

                _logger.LogInformation("Found {Count} new registrations for race {RaceId} from row {LastRow}",
                    sheetRegistrations.Count(), raceId, lastRowIndex + 1);

                foreach (var sheetReg in sheetRegistrations)
                {
                    try
                    {
                        // Find matching distance
                        var distance = race.Distances.FirstOrDefault(d =>
                            d.Distance.Equals(sheetReg.Distance, StringComparison.OrdinalIgnoreCase));

                        if (distance == null)
                        {
                            _logger.LogWarning("Distance '{Distance}' not found for race {RaceId}, row {Row}",
                                sheetReg.Distance, raceId, sheetReg.RowIndex);
                            result.Errors.Add($"Row {sheetReg.RowIndex}: Distance '{sheetReg.Distance}' not found");
                            continue;
                        }

                        // Check if registration already exists (by email + race)
                        var existingReg = await _unitOfWork.Registrations
                            .FirstOrDefaultAsync(r => r.RaceId == raceId && r.Email == sheetReg.Email);

                        if (existingReg != null)
                        {
                            _logger.LogInformation("Registration already exists for {Email} in race {RaceId}",
                                sheetReg.Email, raceId);
                            result.Skipped++;
                            continue;
                        }

                        // Create new registration
                        var registration = new Registration
                        {
                            RaceId = raceId,
                            DistanceId = distance.Id,
                            FullName = sheetReg.FullName,
                            Email = sheetReg.Email,
                            Phone = sheetReg.Phone,
                            BirthYear = sheetReg.BirthYear,
                            Gender = ParseGender(sheetReg.Gender),
                            ShirtCategory = sheetReg.ShirtCategory,  // nhớ map luôn Category
                            ShirtType = sheetReg.ShirtType,          // nhớ map luôn Type
                            ShirtSize = sheetReg.ShirtSize,          // và Size
                            EmergencyContact = sheetReg.EmergencyContact,
                            RegistrationTime = sheetReg.Timestamp,
                            SheetRowIndex = sheetReg.RowIndex,
                            TransactionReference = await GenerateTransactionReference(),
                            PaymentStatus = PaymentStatus.Pending,
                           
                            Fee = distance.Price +
                                  (race.HasShirtSale
                                      ? race.ShirtTypes
                                            .FirstOrDefault(st => st.ShirtType == sheetReg.ShirtType
                                                                && st.ShirtCategory == sheetReg.ShirtCategory)?
                                            .Price ?? 0
                                      : 0)
                        };

                        // ✅ Validate shirt info nếu race có bán áo
                        if (race.HasShirtSale)
                        {
                            if (!ShirtValidator.ValidateShirtSelection(registration, race, out var shirtError))
                            {
                                _logger.LogWarning("Invalid shirt selection for {Email}: {Error}", registration.Email, shirtError);
                                result.Errors.Add($"Row {sheetReg.RowIndex}: {shirtError}");
                                continue; // bỏ qua registration này, không insert DB
                            }
                        }
                        await _unitOfWork.Registrations.AddAsync(registration);
                        newRegistrations.Add(registration);
                        result.Added++;

                        _logger.LogInformation("Added registration for {Email}, reference: {Reference}",
                            registration.Email, registration.TransactionReference);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process registration from row {Row}", sheetReg.RowIndex);
                        result.Errors.Add($"Row {sheetReg.RowIndex}: {ex.Message}");
                    }
                }
                if (newRegistrations.Any())
                {
                    await _unitOfWork.SaveChangesAsync();

                    // enqueue sau khi đã có Id
                    foreach (var reg in newRegistrations)
                    {
                        BackgroundJob.Enqueue<IEmailJob>(
                            x => x.SendRegistrationConfirmationEmailAsync(reg.Id)
                        );
                    }
                }
                // Update sheet config last sync row if using new system
                if (race.HasSheetConfig && result.Added > 0)
                {
                    var maxRowIndex = sheetRegistrations.Any() ? sheetRegistrations.Max(sr => sr.RowIndex) : fromRowIndex;
                    await _unitOfWork.SheetConfigs.UpdateLastSyncRowAsync(race.SheetConfigId!.Value, maxRowIndex);

                    _logger.LogInformation("Updated last sync row to {RowIndex} for sheet config {ConfigId}",
                        maxRowIndex, race.SheetConfigId);
                }
                // Save all changes
                if (result.Added > 0)
                {
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Successfully synced {Added} new registrations for race {RaceId}",
                        result.Added, raceId);
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync registrations for race {RaceId}", raceId);
                result.Error = ex.Message;
            }

            return result;
        }

        public async Task<RegistrationDto?> GetRegistrationAsync(int id)
        {
            var registration = await _unitOfWork.Registrations.GetByIdWithIncludesAsync(id);
            return registration != null ? MapToDto(registration) : null;
        }

        public async Task<IEnumerable<RegistrationDto>> GetRegistrationsByRaceAsync(int raceId)
        {
            var registrations = await _unitOfWork.Registrations.GetByRaceIdAsync(raceId);
            return registrations.Select(MapToDto);
        }

        public async Task ProcessPaymentAsync(int registrationId, PaymentNotificationDto notification)
        {
            try
            {
                var registration = await _unitOfWork.Registrations.GetByIdWithIncludesAsync(registrationId);
                if (registration == null)
                {
                    throw new InvalidOperationException($"Registration {registrationId} not found");
                }

                // Update payment status
                registration.PaymentStatus = PaymentStatus.Paid;

                // Create payment record
                var payment = new Payment
                {
                    RegistrationId = registrationId,
                    TransactionId = notification.TransactionId,
                    Amount = notification.Amount,
                    BankCode = notification.BankCode,
                    TransactionTime = notification.TransactionTime ?? DateTime.Now,
                    Status = PaymentStatus.Paid
                };

                await _unitOfWork.Payments.AddAsync(payment);

                // Generate BIB number
                var bibNumber = await GenerateBibNumberAsync(registrationId);
                registration.BibNumber = bibNumber;

                await _unitOfWork.SaveChangesAsync();
                
                // AUTO-QUEUE BIB NOTIFICATION EMAIL
                BackgroundJob.Enqueue<IEmailJob>(x => x.SendBibNotificationEmailAsync(registrationId));

                _logger.LogInformation("Processed payment for registration {RegistrationId}, BIB: {BibNumber}",
                    registrationId, bibNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process payment for registration {RegistrationId}", registrationId);
                throw;
            }
        }

        public async Task<string> GenerateBibNumberAsync(int registrationId)
        {
            var registration = await _unitOfWork.Registrations.GetByIdWithIncludesAsync(registrationId);
            if (registration == null)
            {
                throw new InvalidOperationException($"Registration {registrationId} not found");
            }

            // Get count of paid registrations for this distance
            var paidCount = await _unitOfWork.Registrations.CountAsync(r =>
                r.DistanceId == registration.DistanceId &&
                r.PaymentStatus == PaymentStatus.Paid &&
                !string.IsNullOrEmpty(r.BibNumber));

            // Generate BIB: Prefix + 3-digit number
            var prefix = registration.Distance.BibPrefix ?? "A";
            var bibNumber = $"{prefix}{(paidCount + 1):000}";

            _logger.LogInformation("Generated BIB number {BibNumber} for registration {RegistrationId}",
                bibNumber, registrationId);

            return bibNumber;
        }

        public async Task<string> GenerateTransactionReference()
        {
            string reference;
            bool exists;

            do
            {
                // Format: RC + YYYYMMDD + 4-digit random
                reference = $"RC{DateTime.Now:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";
                exists = await _unitOfWork.Registrations.AnyAsync(r => r.TransactionReference == reference);
            } while (exists);

            return reference;
        }

        private Gender? ParseGender(string? genderStr)
        {
            if (string.IsNullOrWhiteSpace(genderStr))
                return null;

            return genderStr.ToUpper().StartsWith("M") ? Gender.M :
                   genderStr.ToUpper().StartsWith("F") ? Gender.F : null;
        }

        private RegistrationDto MapToDto(Registration registration)
        {
            return new RegistrationDto
            {
                Id = registration.Id,
                RaceId = registration.RaceId,
                RaceName = registration.Race.Name,
                DistanceId = registration.DistanceId,
                Distance = registration.Distance.Distance,
                Price = registration.Distance.Price,
                FullName = registration.FullName,
                Email = registration.Email,
                Phone = registration.Phone,
                BirthYear = registration.BirthYear,
                Gender = registration.Gender?.ToString(),
                ShirtSize = registration.ShirtSize,
                EmergencyContact = registration.EmergencyContact,
                RegistrationTime = registration.RegistrationTime,
                PaymentStatus = registration.PaymentStatus.ToString(),
                BibNumber = registration.BibNumber,
                BibSentAt = registration.BibSentAt,
                TransactionReference = registration.TransactionReference
            };
        }
    }
}
