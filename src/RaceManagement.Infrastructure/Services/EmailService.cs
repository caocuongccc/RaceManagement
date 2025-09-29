using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using RaceManagement.Core.Entities;
using RaceManagement.Shared.Enums;
using RaceManagement.Core.Interfaces;
using RaceManagement.Core.Models;
using RaceManagement.Shared.DTOs;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RaceManagement.Application.Jobs;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using RaceManagement.Infrastructure.Data;

namespace RaceManagement.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailTemplateService _templateService;
        private readonly IQRCodeService _qrCodeService;
        private readonly EmailConfiguration _emailConfig;
        private readonly RaceManagementDbContext _raceManagementDbContext;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IUnitOfWork unitOfWork,
            IEmailTemplateService templateService,
            IQRCodeService qrCodeService,
            IOptions<EmailConfiguration> emailConfig,
            ILogger<EmailService> logger,
            RaceManagementDbContext raceManagementDbContext
            )
        {
            _unitOfWork = unitOfWork;
            _templateService = templateService;
            _qrCodeService = qrCodeService;
            _emailConfig = emailConfig.Value;
            _logger = logger;
            _raceManagementDbContext = raceManagementDbContext;
        }
        // Update GenerateQRCodeAsync to use the service
        //public async Task<byte[]> GenerateQRCodeAsync(string content)
        //{
        //    return await _qrCodeService.GenerateQRCodeAsync(content);
        //}

        //public async Task<byte[]> GeneratePaymentQRCodeAsync(RegistrationDto registration)
        //{
        //    return await _qrCodeService.GeneratePaymentQRCodeAsync(registration);
        //}
        public async Task<EmailResult> SendEmailAsync(EmailRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new EmailResult();
            
            try
            {
                using var message = new MimeMessage();

                // Setup message
                message.From.Add(new MailboxAddress(
                    _emailConfig.Smtp.FromName,
                    _emailConfig.Smtp.FromEmail));
                message.To.Add(new MailboxAddress(request.ToName ?? request.To, request.To));
                message.Subject = request.Subject;

                // Build message body
                var bodyBuilder = new BodyBuilder();

                if (!string.IsNullOrEmpty(request.PlainTextContent))
                    bodyBuilder.TextBody = request.PlainTextContent;

                if (!string.IsNullOrEmpty(request.HtmlContent))
                    bodyBuilder.HtmlBody = request.HtmlContent;

                // Add attachments
                foreach (var attachment in request.Attachments)
                {
                    if (attachment.IsInline)
                    {
                        var inline = bodyBuilder.LinkedResources.Add(
                            attachment.FileName,
                            attachment.Content,
                            ContentType.Parse(attachment.ContentType));

                        if (!string.IsNullOrEmpty(attachment.ContentId))
                            inline.ContentId = attachment.ContentId;
                    }
                    else
                    {
                        bodyBuilder.Attachments.Add(
                            attachment.FileName,
                            attachment.Content,
                            ContentType.Parse(attachment.ContentType));
                    }
                }

                message.Body = bodyBuilder.ToMessageBody();

                // Set priority
                message.Priority = request.Priority switch
                {
                    EmailPriority.High => MessagePriority.Urgent,
                    EmailPriority.Low => MessagePriority.NonUrgent,
                    _ => MessagePriority.Normal
                };

                // Send email
                using var client = new SmtpClient();

                await client.ConnectAsync(
                    _emailConfig.Smtp.Host,
                    _emailConfig.Smtp.Port,
                    _emailConfig.Smtp.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

                if (!string.IsNullOrEmpty(_emailConfig.Smtp.Username))
                {
                    await client.AuthenticateAsync(_emailConfig.Smtp.Username, _emailConfig.Smtp.Password);
                }

                var messageId = await client.SendAsync(message);
                await client.DisconnectAsync(true);

                stopwatch.Stop();

                result.IsSuccess = true;
                result.MessageId = messageId;
                result.ProcessingTime = stopwatch.Elapsed;

                // Log success
                if (request.RegistrationId.HasValue && request.EmailType.HasValue)
                {
                    result.EmailLogId = await LogEmailAsync(
                        request.RegistrationId.Value,
                        request.To,
                        request.ToName ?? request.To,
                        request.Subject,
                        request.EmailType.Value,
                        EmailStatus.Sent,
                        messageId: messageId,
                        processingTime: stopwatch.Elapsed
                    );
                }

                _logger.LogInformation("Email sent successfully to {To}, MessageId: {MessageId}", request.To, messageId);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                result.ProcessingTime = stopwatch.Elapsed;

                // Log failure
                if (request.RegistrationId.HasValue && request.EmailType.HasValue)
                {
                    result.EmailLogId = await LogEmailAsync(
                        request.RegistrationId.Value,
                        request.To,
                        request.ToName ?? request.To,
                        request.Subject,
                        request.EmailType.Value,
                        EmailStatus.Failed,
                        errorMessage: ex.Message,
                        processingTime: stopwatch.Elapsed
                    );
                }

                _logger.LogError(ex, "Failed to send email to {To}", request.To);
            }

            return result;
        }
        //public async Task<EmailResult> SendRegistrationConfirmationAsync(int registrationId)
        //{
        //    var registration = await _raceManagementDbContext.Registrations
        //        .Include(r => r.Race)
        //        .Include(r => r.Distance)
        //        .FirstOrDefaultAsync(r => r.Id == registrationId);

        //    if (registration == null)
        //        return new EmailResult { IsSuccess = false, ErrorMessage = "Registration not found" };

        //    // Tạo QR code VietQR với tổng số tiền
        //    var qrCodeBytes = await _qrCodeService.GeneratePaymentQRCodeAsync(
        //        registration.TransactionReference,
        //        registration.TotalAmount,
        //        registration.Race
        //    );

        //    // Convert QR code sang Base64 để nhúng vào HTML
        //    var qrBase64 = $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";

        //    // Render template (thêm placeholder {{QrCode}} vào template)
        //    var template = await _templateService.RenderTemplateAsync("registration-confirmation", new
        //    {
        //        registration.FullName,
        //        registration.Race.Name,
        //        registration.Distance.Distance,
        //        registration.TransactionReference,
        //        Price = registration.TotalAmount.ToString("N0"),
        //        registration.GetShirtFullDescription(),
        //        QrCode = qrBase64
        //    });

        //    var emailRequest = new EmailRequest
        //    {
        //        To = registration.Email,
        //        Subject = $"Xác nhận đăng ký {registration.Race.Name}",
        //        HtmlBody = template
        //    };

        //    return await _emailSender.SendEmailAsync(emailRequest);
        //}


        public async Task<EmailResult> SendRegistrationConfirmationAsync(int registrationId)
        {
            //var registration = await _unitOfWork.Registrations.GetByIdWithIncludesAsync(registrationId);            //var registration = await _unitOfWork.Registrations.GetByIdWithIncludesAsync(registrationId);
            var registration = await _raceManagementDbContext.Registrations
                .Include(r => r.Race)
                .Include(r => r.Distance)
                //.Include(r => r.Race.ShirtTypes)
                .FirstOrDefaultAsync(r => r.Id == registrationId);
            if (registration == null)
            {
                return new EmailResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Registration {registrationId} not found"
                };
            }

            var registrationDto = MapToRegistrationDto(registration);

            // Render template
            var htmlContent = await _templateService.RenderTemplateAsync("registration-confirmation", registrationDto);
            var subject = _templateService.GetTemplateSubject("registration-confirmation", registrationDto);

            // Generate payment QR code
            var qrCodeBytes = await _qrCodeService.GeneratePaymentQRCodeAsync(
                registration.TransactionReference,
                registration.TotalAmount,
                registration.Race
            );

            var emailRequest = new EmailRequest
            {
                To = registration.Email,
                ToName = registration.FullName,
                Subject = subject,
                HtmlContent = htmlContent,
                RegistrationId = registrationId,
                EmailType = EmailType.RegistrationConfirm,
                Priority = EmailPriority.High,
                Attachments = new List<EmailAttachment>
            {
                new EmailAttachment
                {
                    FileName = "payment-qr.png",
                    Content = qrCodeBytes,
                    ContentType = "image/png",
                    IsInline = true,
                    ContentId = "payment-qr"
                }
            }
            };

            return await SendEmailAsync(emailRequest);
        }

        public async Task<EmailResult> SendBibNotificationAsync(int registrationId)
        {
            var registration = await _unitOfWork.Registrations.GetByIdWithIncludesAsync(registrationId);
            if (registration == null)
            {
                return new EmailResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Registration {registrationId} not found"
                };
            }

            if (string.IsNullOrEmpty(registration.BibNumber))
            {
                return new EmailResult
                {
                    IsSuccess = false,
                    ErrorMessage = "BIB number not assigned yet"
                };
            }

            var registrationDto = MapToRegistrationDto(registration);

            // Render template
            var htmlContent = await _templateService.RenderTemplateAsync("bib-notification", registrationDto);
            var subject = _templateService.GetTemplateSubject("bib-notification", registrationDto);

            // Generate BIB QR code
            var bibQrCodeBytes = await _qrCodeService.GenerateBibQRCodeAsync(
                registration.BibNumber,
                registration.Race.Name
            );

            var emailRequest = new EmailRequest
            {
                To = registration.Email,
                ToName = registration.FullName,
                Subject = subject,
                HtmlContent = htmlContent,
                RegistrationId = registrationId,
                EmailType = EmailType.BibNumber,
                Priority = EmailPriority.High,
                Attachments = new List<EmailAttachment>
            {
                new EmailAttachment
                {
                    FileName = "bib-qr.png",
                    Content = bibQrCodeBytes,
                    ContentType = "image/png",
                    IsInline = true,
                    ContentId = "bib-qr"
                }
            }
            };

            var result = await SendEmailAsync(emailRequest);

            // Update BibSentAt if successful
            if (result.IsSuccess)
            {
                registration.BibSentAt = DateTime.Now;
                _unitOfWork.Registrations.Update(registration);
                await _unitOfWork.SaveChangesAsync();
            }

            return result;
        }

        public async Task<EmailResult> SendPaymentReminderAsync(int registrationId)
        {
            var registration = await _unitOfWork.Registrations.GetByIdWithIncludesAsync(registrationId);
            if (registration == null)
            {
                return new EmailResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Registration {registrationId} not found"
                };
            }

            if (registration.PaymentStatus == PaymentStatus.Paid)
            {
                return new EmailResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Registration already paid"
                };
            }

            var registrationDto = MapToRegistrationDto(registration);

            // Render template
            var htmlContent = await _templateService.RenderTemplateAsync("payment-reminder", registrationDto);
            var subject = _templateService.GetTemplateSubject("payment-reminder", registrationDto);

            var emailRequest = new EmailRequest
            {
                To = registration.Email,
                ToName = registration.FullName,
                Subject = subject,
                HtmlContent = htmlContent,
                RegistrationId = registrationId,
                EmailType = EmailType.PaymentReminder,
                Priority = EmailPriority.Normal
            };

            return await SendEmailAsync(emailRequest);
        }

        public async Task<EmailResult> SendRaceDayInfoAsync(int registrationId)
        {
            var registration = await _unitOfWork.Registrations.GetByIdWithIncludesAsync(registrationId);
            if (registration == null)
            {
                return new EmailResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Registration {registrationId} not found"
                };
            }

            var registrationDto = MapToRegistrationDto(registration);

            // Render template
            var htmlContent = await _templateService.RenderTemplateAsync("race-day-info", registrationDto);
            var subject = _templateService.GetTemplateSubject("race-day-info", registrationDto);

            var emailRequest = new EmailRequest
            {
                To = registration.Email,
                ToName = registration.FullName,
                Subject = subject,
                HtmlContent = htmlContent,
                RegistrationId = registrationId,
                EmailType = EmailType.RaceDayInfo,
                Priority = EmailPriority.Normal
            };

            return await SendEmailAsync(emailRequest);
        }

        // Queue management
        public async Task QueueEmailAsync(int registrationId, EmailType emailType, DateTime? scheduledAt = null)
        {
            var registration = await _unitOfWork.Registrations.GetByIdWithIncludesAsync(registrationId);
            if (registration == null)
            {
                throw new ArgumentException($"Registration {registrationId} not found");
            }

            // Check if email already queued
            var existingEmail = await _unitOfWork.EmailQueues.GetByRegistrationAndTypeAsync(registrationId, emailType);
            if (existingEmail != null && existingEmail.Status == EmailStatus.Pending)
            {
                _logger.LogInformation("Email {EmailType} already queued for registration {RegistrationId}",
                    emailType, registrationId);
                return;
            }

            var subject = _templateService.GetTemplateSubject(GetTemplateName(emailType), MapToRegistrationDto(registration));

            var emailQueue = new EmailQueue
            {
                RegistrationId = registrationId,
                RecipientEmail = registration.Email,
                RecipientName = registration.FullName,
                Subject = subject,
                EmailType = emailType,
                Priority = GetEmailPriority(emailType),
                ScheduledAt = scheduledAt,
                Status = EmailStatus.Pending
            };

            await _unitOfWork.EmailQueues.AddAsync(emailQueue);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Queued email {EmailType} for registration {RegistrationId}, scheduled at {ScheduledAt}",
                emailType, registrationId, scheduledAt?.ToString() ?? "immediate");
        }

        public async Task<EmailQueueStatusDto> GetQueueStatusAsync()
        {
            var statusCounts = await _unitOfWork.EmailQueues.GetStatusCountsAsync();
            var recentItems = await _unitOfWork.EmailQueues.GetQueueStatusAsync(20);

            return new EmailQueueStatusDto
            {
                PendingEmails = statusCounts.GetValueOrDefault(EmailStatus.Pending, 0),
                ProcessingEmails = statusCounts.GetValueOrDefault(EmailStatus.Processing, 0),
                CompletedEmails = statusCounts.GetValueOrDefault(EmailStatus.Sent, 0),
                FailedEmails = statusCounts.GetValueOrDefault(EmailStatus.Failed, 0),
                NextScheduledEmail = recentItems
                    .Where(e => e.Status == EmailStatus.Pending && e.ScheduledAt.HasValue)
                    .OrderBy(e => e.ScheduledAt)
                    .FirstOrDefault()?.ScheduledAt,
                RecentItems = recentItems.Select(MapToEmailQueueItemDto).ToList()
            };
        }

        // Bulk operations
        public async Task<BulkEmailResult> SendBulkEmailAsync(BulkEmailRequest request)
        {
            var result = new BulkEmailResult
            {
                TotalEmails = request.Emails.Count
            };

            var stopwatch = Stopwatch.StartNew();

            foreach (var batch in request.Emails.Chunk(request.BatchSize))
            {
                var batchTasks = batch.Select(email => SendEmailAsync(email));
                var batchResults = await Task.WhenAll(batchTasks);

                result.Results.AddRange(batchResults);
                result.SuccessfulEmails += batchResults.Count(r => r.IsSuccess);
                result.FailedEmails += batchResults.Count(r => !r.IsSuccess);

                if (request.StopOnFirstError && batchResults.Any(r => !r.IsSuccess))
                {
                    _logger.LogWarning("Stopping bulk email send due to error in batch");
                    break;
                }

                // Delay between batches to avoid rate limiting
                if (batch != request.Emails.TakeLast(request.BatchSize))
                {
                    await Task.Delay(request.DelayBetweenBatches);
                }
            }

            stopwatch.Stop();
            result.TotalProcessingTime = stopwatch.Elapsed;

            _logger.LogInformation("Bulk email completed: {Successful}/{Total} successful",
                result.SuccessfulEmails, result.TotalEmails);

            return result;
        }

        public async Task<BulkEmailResult> SendRaceNotificationsAsync(int raceId, EmailType emailType)
        {
            var race = await _unitOfWork.Races.GetRaceWithFullDetailsAsync(raceId);
            if (race == null)
            {
                throw new ArgumentException($"Race {raceId} not found");
            }

            // Filter registrations based on email type
            var registrations = FilterRegistrationsByEmailType(race.Registrations, emailType);

            var emailRequests = new List<EmailRequest>();

            foreach (var registration in registrations)
            {
                try
                {
                    var emailRequest = await BuildEmailRequestAsync(registration, emailType);
                    emailRequests.Add(emailRequest);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to build email request for registration {RegistrationId}", registration.Id);
                }
            }

            var bulkRequest = new BulkEmailRequest
            {
                Emails = emailRequests,
                BatchSize = _emailConfig.Jobs.BatchSize,
                DelayBetweenBatches = _emailConfig.Jobs.DelayBetweenBatches,
                StopOnFirstError = false
            };

            return await SendBulkEmailAsync(bulkRequest);
        }

        // Template and QR code methods
        public async Task<string> RenderTemplateAsync(string templateName, object model)
        {
            return await _templateService.RenderTemplateAsync(templateName, model);
        }

        public async Task<bool> ValidateTemplateAsync(string templateName)
        {
            return await _templateService.TemplateExistsAsync(templateName);
        }

        public async Task<byte[]> GenerateQRCodeAsync(string content)
        {
            return await _qrCodeService.GenerateQRCodeAsync(content);
        }

        public async Task<byte[]> GeneratePaymentQRCodeAsync(RegistrationDto registration)
        {
            return await _qrCodeService.GeneratePaymentQRCodeAsync(
                registration.TransactionReference,
                registration.Price,
                $"{registration.RaceName} - {registration.Distance}"
            );
        }

        // Helper methods
        private async Task<EmailRequest> BuildEmailRequestAsync(Registration registration, EmailType emailType)
        {
            var registrationDto = MapToRegistrationDto(registration);
            var templateName = GetTemplateName(emailType);

            var htmlContent = await _templateService.RenderTemplateAsync(templateName, registrationDto);
            var subject = _templateService.GetTemplateSubject(templateName, registrationDto);

            var emailRequest = new EmailRequest
            {
                To = registration.Email,
                ToName = registration.FullName,
                Subject = subject,
                HtmlContent = htmlContent,
                RegistrationId = registration.Id,
                EmailType = emailType,
                Priority = GetEmailPriority(emailType)
            };

            // Add attachments based on email type
            if (emailType == EmailType.RegistrationConfirm)
            {
                var qrCodeBytes = await _qrCodeService.GeneratePaymentQRCodeAsync(
                    registration.TransactionReference,
                    registration.Distance.Price,
                    $"{registration.Race.Name} - {registration.Distance.Distance}"
                );

                emailRequest.Attachments.Add(new EmailAttachment
                {
                    FileName = "payment-qr.png",
                    Content = qrCodeBytes,
                    ContentType = "image/png",
                    IsInline = true,
                    ContentId = "payment-qr"
                });
            }
            else if (emailType == EmailType.BibNumber && !string.IsNullOrEmpty(registration.BibNumber))
            {
                var bibQrCodeBytes = await _qrCodeService.GenerateBibQRCodeAsync(
                    registration.BibNumber,
                    registration.Race.Name
                );

                emailRequest.Attachments.Add(new EmailAttachment
                {
                    FileName = "bib-qr.png",
                    Content = bibQrCodeBytes,
                    ContentType = "image/png",
                    IsInline = true,
                    ContentId = "bib-qr"
                });
            }

            return emailRequest;
        }

        private IEnumerable<Registration> FilterRegistrationsByEmailType(IEnumerable<Registration> registrations, EmailType emailType)
        {
            return emailType switch
            {
                EmailType.RegistrationConfirm => registrations,
                EmailType.BibNumber => registrations.Where(r => r.PaymentStatus == PaymentStatus.Paid && !string.IsNullOrEmpty(r.BibNumber)),
                EmailType.PaymentReminder => registrations.Where(r => r.PaymentStatus == PaymentStatus.Pending),
                EmailType.RaceDayInfo => registrations.Where(r => r.PaymentStatus == PaymentStatus.Paid),
                _ => registrations
            };
        }

        private async Task<int> LogEmailAsync(
            int registrationId,
            string recipientEmail,
            string recipientName,
            string subject,
            EmailType emailType,
            EmailStatus status,
            string? messageId = null,
            string? errorMessage = null,
            TimeSpan? processingTime = null)
        {
            var emailLog = new EmailLog
            {
                RegistrationId = registrationId,
                RecipientEmail = recipientEmail,
                RecipientName = recipientName,
                Subject = subject,
                EmailType = emailType,
                Status = status,
                SentAt = DateTime.Now,
                ProcessingTime = processingTime ?? TimeSpan.Zero,
                MessageId = messageId,
                ErrorMessage = errorMessage,
                TemplateName = GetTemplateName(emailType)
            };

            await _unitOfWork.EmailLogs.AddAsync(emailLog);
            await _unitOfWork.SaveChangesAsync();

            return emailLog.Id;
        }

        private RegistrationDto MapToRegistrationDto(Registration registration)
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
                BibName = registration.BibName,
                Email = registration.Email,
                Phone = registration.Phone,
                BirthYear = registration.BirthYear,
                DateOfBirth = registration.DateOfBirth,
                Gender = registration.Gender?.ToString(),
                ShirtCategory = registration.ShirtCategory,
                ShirtSize = registration.ShirtSize,
                ShirtType = registration.ShirtType,
                EmergencyContact = registration.EmergencyContact,
                RegistrationTime = registration.RegistrationTime,
                PaymentStatus = registration.PaymentStatus.ToString(),
                BibNumber = registration.BibNumber,
                BibSentAt = registration.BibSentAt,
                TransactionReference = registration.TransactionReference
            };
        }

        private EmailQueueItemDto MapToEmailQueueItemDto(EmailQueue emailQueue)
        {
            return new EmailQueueItemDto
            {
                Id = emailQueue.Id,
                RegistrationId = emailQueue.RegistrationId,
                RecipientEmail = emailQueue.RecipientEmail,
                RecipientName = emailQueue.RecipientName,
                EmailType = emailQueue.EmailType,
                Status = emailQueue.Status,
                CreatedAt = emailQueue.CreatedAt,
                ScheduledAt = emailQueue.ScheduledAt,
                ProcessedAt = emailQueue.ProcessedAt,
                RetryCount = emailQueue.RetryCount,
                ErrorMessage = emailQueue.ErrorMessage
            };
        }

        private string GetTemplateName(EmailType emailType)
        {
            return emailType switch
            {
                EmailType.RegistrationConfirm => "registration-confirmation",
                EmailType.BibNumber => "bib-notification",
                EmailType.PaymentReminder => "payment-reminder",
                EmailType.RaceDayInfo => "race-day-info",
                _ => "generic"
            };
        }

        private EmailPriority GetEmailPriority(EmailType emailType)
        {
            return emailType switch
            {
                EmailType.RegistrationConfirm => EmailPriority.High,
                EmailType.BibNumber => EmailPriority.High,
                EmailType.PaymentReminder => EmailPriority.Normal,
                EmailType.RaceDayInfo => EmailPriority.Normal,
                _ => EmailPriority.Normal
            };
        }

        // Wrapper methods cho Hangfire
        // Wrapper methods cho Hangfire
        public void SendRegistrationConfirmationEmail(int registrationId)
        {
            BackgroundJob.Enqueue<IEmailJob>(job => job.SendRegistrationConfirmationEmailAsync(registrationId));
        }

        public void SendBibNotificationEmail(int registrationId)
        {
            BackgroundJob.Enqueue<IEmailJob>(job => job.SendBibNotificationEmailAsync(registrationId));
        }

        public void SendPaymentReminderEmails(int raceId)
        {
            BackgroundJob.Enqueue<IEmailJob>(job => job.SendPaymentReminderEmailsAsync(raceId));
        }

        public void SendRaceDayInfoEmails(int raceId)
        {
            BackgroundJob.Enqueue<IEmailJob>(job => job.SendRaceDayInfoEmailsAsync(raceId));
        }

        public void ProcessPendingEmails()
        {
            BackgroundJob.Enqueue<IEmailJob>(job => job.ProcessPendingEmailsAsync());
        }

    }
}
