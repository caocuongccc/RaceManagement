using Hangfire;
using Microsoft.Extensions.Logging;
using RaceManagement.Application.Jobs;
using RaceManagement.Core.Entities;
using RaceManagement.Shared.Enums;
using RaceManagement.Core.Interfaces;
using RaceManagement.Shared.DTOs;

namespace RaceManagement.Application.Services
{
    public class RaceService : IRaceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IGoogleSheetsService _googleSheetsService;
        private readonly ILogger<RaceService> _logger;

        public RaceService(
            IUnitOfWork unitOfWork,
            IGoogleSheetsService googleSheetsService,
            ILogger<RaceService> logger,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _googleSheetsService = googleSheetsService;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<RaceDto> CreateRaceAsync(CreateRaceDto dto)
        {
            try
            {
                // Get sheet config và verify
                var sheetConfig = await _unitOfWork.SheetConfigs.GetConfigWithCredentialAsync(dto.SheetConfigId);
                if (sheetConfig == null || !sheetConfig.IsActive)
                {
                    throw new InvalidOperationException($"Sheet config with ID {dto.SheetConfigId} not found or inactive");
                }

                // Test connection trước khi tạo race
                var isConnected = await _googleSheetsService.TestConnectionAsync(
                    sheetConfig.SpreadsheetId,
                    sheetConfig.GetCredentialPath());

                if (!isConnected)
                {
                    throw new InvalidOperationException($"Cannot connect to Google Sheet using config: {sheetConfig.Name}");
                }

                var race = new Race
                {
                    Name = dto.Name,
                    RaceDate = dto.RaceDate,
                    Email = dto.Email,
                    EmailPassword = dto.EmailPassword, // Should encrypt this
                    SheetConfigId = dto.SheetConfigId,  // NEW - use sheet config instead of direct sheet ID
                    Status = RaceStatus.Active,
                    HasShirtSale = dto.HasShirtSale,
                    GoogleCredentialPath = sheetConfig.GetCredentialPath(),
                    BankName = dto.BankName,
                    BankAccountNo = dto.BankAccountNo,
                    BankAccountHolder = dto.BankAccountHolder
                };

                // Add distances
                foreach (var distanceDto in dto.Distances)
                {
                    race.Distances.Add(new RaceDistance
                    {
                        Distance = distanceDto.Distance,
                        Price = distanceDto.Price,
                        BibPrefix = distanceDto.BibPrefix,
                        MaxParticipants = distanceDto.MaxParticipants
                    });
                }

                // Add shirt types
                foreach (var shirtDto in dto.ShirtTypes)
                {
                    race.ShirtTypes.Add(new RaceShirtType
                    {
                        ShirtCategory = shirtDto.ShirtCategory,
                        ShirtType = shirtDto.ShirtType,
                        AvailableSizes = shirtDto.AvailableSizes,
                        Price = shirtDto.Price,
                        IsActive = shirtDto.IsActive
                    });
                }

                await _unitOfWork.Races.AddAsync(race);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created race {RaceId}: {RaceName} with sheet config {SheetConfigId}",
                    race.Id, race.Name, race.SheetConfigId);

                // 1. Clone sheet gốc để tạo Payment Tracking Sheet
                var paymentSheetId = await _googleSheetsService.CreatePaymentTrackingSheetAsync(
                    sheetConfig.SpreadsheetId,
                    race.Name,
                    sheetConfig.GetCredentialPath()
                );
                race.PaymentSheetId = paymentSheetId;

                // 2. Lưu lại thay đổi
                await _unitOfWork.SaveChangesAsync();

                // 3. Bật auto sync registrations từ sheet
                // Đặt recurring job với Hangfire (ví dụ 30 giây/lần)
                RecurringJob.AddOrUpdate<IRegistrationService>(
                    $"sync-registrations-race-{race.Id}",
                    x => x.SyncRegistrationsFromSheetAsync(race.Id),
                    Cron.MinuteInterval(1)  // mỗi 1 phút, bạn có thể chỉnh 30s
                );

                return MapToDto(race);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create race: {RaceName}", dto.Name);
                throw;
            }
        }

        public async Task<RaceDto?> GetRaceAsync(int id)
        {
            var race = await _unitOfWork.Races.GetRaceWithDistancesAsync(id);
            return race != null ? MapToDto(race) : null;
        }

        public async Task<IEnumerable<RaceDto>> GetActiveRacesAsync()
        {
            var races = await _unitOfWork.Races.GetActiveRacesAsync();
            return races.Select(MapToDto);
        }

        public async Task<RaceStatisticsDto> GetRaceStatisticsAsync(int raceId)
        {
            var registrations = await _unitOfWork.Registrations.GetByRaceIdAsync(raceId);

            var stats = new RaceStatisticsDto
            {
                RaceId = raceId,
                TotalRegistrations = registrations.Count(),
                PendingPayments = registrations.Count(r => r.PaymentStatus == PaymentStatus.Pending),
                PaidRegistrations = registrations.Count(r => r.PaymentStatus == PaymentStatus.Paid),
                BibsGenerated = registrations.Count(r => !string.IsNullOrEmpty(r.BibNumber)),
                BibsSent = registrations.Count(r => r.BibSentAt.HasValue),
                TotalRevenue = registrations
                    .Where(r => r.PaymentStatus == PaymentStatus.Paid)
                    .Sum(r => r.Distance.Price)
            };

            return stats;
        }

        // Fix 6: Update MapToDto method in RaceService
        private RaceDto MapToDto(Race race)
        {
            return new RaceDto
            {
                Id = race.Id,
                Name = race.Name,
                RaceDate = race.RaceDate,
                Email = race.Email,
                SheetId = race.SheetId, // Legacy field
                SheetConfigId = race.SheetConfigId, // NEW
                SheetConfigName = race.SheetConfig?.Name, // NEW
                CredentialName = race.SheetConfig?.Credential?.Name, // NEW
                PaymentSheetId = race.PaymentSheetId,
                Status = race.Status.ToString(),
                CreatedAt = race.CreatedAt,
                Distances = race.Distances.Select(d => new RaceDistanceDto
                {
                    Id = d.Id,
                    Distance = d.Distance,
                    Price = d.Price,
                    BibPrefix = d.BibPrefix,
                    MaxParticipants = d.MaxParticipants
                }).ToList(),
                ShirtTypes = race.ShirtTypes.Select(st => new RaceShirtTypeDto
                {
                    Id = st.Id,
                    ShirtCategory = st.ShirtCategory,
                    ShirtType = st.ShirtType,
                    AvailableSizes = st.AvailableSizes,
                    Price = st.Price,
                    IsActive = st.IsActive
                }).ToList()
            };
        }

        public async Task<BulkEmailResult> SendRaceNotificationsAsync(int raceId, EmailType emailType, DateTime? scheduledAt = null)
        {
            try
            {
                var race = await _unitOfWork.Races.GetRaceWithFullDetailsAsync(raceId); 
                if (race == null)
                {
                    throw new ArgumentException($"Race {raceId} not found");
                }

                // Filter registrations based on email type
                var eligibleRegistrations = FilterRegistrationsByEmailType(race.Registrations, emailType);

                if (!eligibleRegistrations.Any())
                {
                    _logger.LogWarning("No eligible registrations found for email type {EmailType} in race {RaceId}",
                        emailType, raceId);

                    return new BulkEmailResult
                    {
                        TotalEmails = 0,
                        SuccessfulEmails = 0,
                        FailedEmails = 0
                    };
                }

                // Queue emails for processing
                foreach (var registration in eligibleRegistrations)
                {
                    if (scheduledAt.HasValue)
                    {
                        await _emailService.QueueEmailAsync(registration.Id, emailType, scheduledAt);
                    }
                    else
                    {
                        switch (emailType)
                        {
                            case EmailType.RegistrationConfirm:
                                BackgroundJob.Enqueue<IEmailJob>(x => x.SendRegistrationConfirmationEmailAsync(registration.Id));
                                break;

                            case EmailType.BibNumber:
                                BackgroundJob.Enqueue<IEmailJob>(x => x.SendBibNotificationEmailAsync(registration.Id));
                                break;

                            case EmailType.PaymentReminder:
                                BackgroundJob.Enqueue<IEmailJob>(x => x.SendPaymentReminderEmailsAsync(raceId));
                                break;

                            case EmailType.RaceDayInfo:
                                BackgroundJob.Enqueue<IEmailJob>(x => x.SendRaceDayInfoEmailsAsync(raceId));
                                break;

                            default:
                                BackgroundJob.Enqueue<IEmailJob>(x => x.ProcessPendingEmailsAsync());
                                break;
                        }
                        //BackgroundJob.Enqueue<IEmailJob>(x =>
                        //    emailType switch
                        //    {
                        //        EmailType.RegistrationConfirm => x.SendRegistrationConfirmationEmailAsync(registration.Id),
                        //        EmailType.BibNumber => x.SendBibNotificationEmailAsync(registration.Id),
                        //        EmailType.PaymentReminder => x.SendPaymentReminderEmailsAsync(raceId),
                        //        EmailType.RaceDayInfo => x.SendRaceDayInfoEmailsAsync(raceId),
                        //        _ => x.ProcessPendingEmailsAsync()
                        //    });
                    }
                }

                _logger.LogInformation("Queued {Count} {EmailType} emails for race {RaceId}",
                    eligibleRegistrations.Count(), emailType, raceId);

                return new BulkEmailResult
                {
                    TotalEmails = eligibleRegistrations.Count(),
                    SuccessfulEmails = eligibleRegistrations.Count(), // Queued successfully
                    FailedEmails = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue race notifications for race {RaceId}", raceId);
                throw;
            }
        }

        private IEnumerable<Registration> FilterRegistrationsByEmailType(IEnumerable<Registration> registrations, EmailType emailType)
        {
            return emailType switch
            {
                EmailType.RegistrationConfirm => registrations,
                EmailType.BibNumber => registrations.Where(r =>
                    r.PaymentStatus == PaymentStatus.Paid &&
                    !string.IsNullOrEmpty(r.BibNumber) &&
                    !r.BibSentAt.HasValue),
                EmailType.PaymentReminder => registrations.Where(r =>
                    r.PaymentStatus == PaymentStatus.Pending &&
                    r.RegistrationTime < DateTime.Now.AddHours(-24)), // Only if registered more than 24h ago
                EmailType.RaceDayInfo => registrations.Where(r =>
                    r.PaymentStatus == PaymentStatus.Paid),
                _ => registrations
            };
        }

        public async Task<Race?> GetRaceWithFullDetailsAsync(int raceId)
        {
            return await _unitOfWork.Races.GetRaceWithFullDetailsAsync(raceId);
        }
    }
}
