using Hangfire;
using Microsoft.Extensions.Logging;
using RaceManagement.Abstractions.Enums;
using RaceManagement.Core.Interfaces;
using RaceManagement.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Application.Jobs
{
    public class EmailJob : IEmailJob
    {
        private readonly IEmailQueueProcessor _emailQueueProcessor; // Thay đổi từ IEmailService
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmailJob> _logger;

        public EmailJob(
            IEmailQueueProcessor emailQueueProcessor, // Thay đổi
            IUnitOfWork unitOfWork,
            ILogger<EmailJob> logger)
        {
            _emailQueueProcessor = emailQueueProcessor;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task ProcessPendingEmailsAsync()
        {
            try
            {
                _logger.LogInformation("Starting to process pending emails");

                var pendingEmails = await _unitOfWork.EmailQueues.GetPendingEmailsAsync(10);

                if (!pendingEmails.Any())
                {
                    _logger.LogDebug("No pending emails to process");
                    return;
                }

                _logger.LogInformation("Found {Count} pending emails to process", pendingEmails.Count());

                // Mark emails as processing to avoid duplicate processing
                await _unitOfWork.EmailQueues.MarkAsProcessingAsync(pendingEmails.Select(e => e.Id));

                foreach (var emailQueue in pendingEmails)
                {
                    try
                    {
                        // Sử dụng EmailQueueProcessor thay vì EmailService
                        EmailResult result = emailQueue.EmailType switch
                        {
                            EmailType.RegistrationConfirm => await _emailQueueProcessor.SendRegistrationConfirmationFromQueueAsync(emailQueue.RegistrationId),
                            EmailType.BibNumber => await _emailQueueProcessor.SendBibNotificationFromQueueAsync(emailQueue.RegistrationId),
                            EmailType.PaymentReminder => await _emailQueueProcessor.SendPaymentReminderFromQueueAsync(emailQueue.RegistrationId),
                            EmailType.RaceDayInfo => await _emailQueueProcessor.SendRaceDayInfoFromQueueAsync(emailQueue.RegistrationId),
                            _ => throw new NotSupportedException($"Email type {emailQueue.EmailType} not supported")
                        };

                        if (result.IsSuccess)
                        {
                            emailQueue.MarkAsSent(result.MessageId);
                            _logger.LogInformation("Successfully sent email {EmailId} to {Email}",
                                emailQueue.Id, emailQueue.RecipientEmail);
                        }
                        else
                        {
                            emailQueue.IncrementRetry(result.ErrorMessage ?? "Unknown error");
                            _logger.LogWarning("Failed to send email {EmailId}: {Error}",
                                emailQueue.Id, result.ErrorMessage);

                            // Schedule retry if allowed
                            if (emailQueue.CanRetry)
                            {
                                BackgroundJob.Schedule<IEmailJob>(
                                    x => x.ProcessPendingEmailsAsync(),
                                    TimeSpan.FromMinutes(5));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        emailQueue.IncrementRetry(ex.Message);
                        _logger.LogError(ex, "Exception processing email {EmailId}", emailQueue.Id);
                    }

                    _unitOfWork.EmailQueues.Update(emailQueue);
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Completed processing pending emails");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pending emails");
                throw;
            }
        }

        [AutomaticRetry(Attempts = 2)]
        public async Task ProcessScheduledEmailsAsync()
        {
            try
            {
                var scheduledEmails = await _unitOfWork.EmailQueues.GetScheduledEmailsAsync(DateTime.Now);

                if (!scheduledEmails.Any())
                    return;

                _logger.LogInformation("Found {Count} scheduled emails ready to send", scheduledEmails.Count());

                foreach (var email in scheduledEmails)
                {
                    // Reset scheduled time and mark as pending for immediate processing
                    email.ScheduledAt = null;
                    email.Status = EmailStatus.Pending;
                    _unitOfWork.EmailQueues.Update(email);
                }

                await _unitOfWork.SaveChangesAsync();

                // Trigger immediate processing of now-pending emails
                BackgroundJob.Enqueue<IEmailJob>(x => x.ProcessPendingEmailsAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled emails");
                throw;
            }
        }

        [AutomaticRetry(Attempts = 1)]
        public async Task RetryFailedEmailsAsync()
        {
            try
            {
                var failedEmails = await _unitOfWork.EmailQueues.GetFailedEmailsForRetryAsync(5);

                if (!failedEmails.Any())
                    return;

                _logger.LogInformation("Found {Count} failed emails to retry", failedEmails.Count());

                foreach (var email in failedEmails)
                {
                    // Reset to pending for retry
                    email.Status = EmailStatus.Pending;
                    _unitOfWork.EmailQueues.Update(email);
                }

                await _unitOfWork.SaveChangesAsync();

                // Process the retries
                BackgroundJob.Enqueue<IEmailJob>(x => x.ProcessPendingEmailsAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying failed emails");
                throw;
            }
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task SendRegistrationConfirmationEmailAsync(int registrationId)
        {
            try
            {
                var result = await _emailQueueProcessor.SendRegistrationConfirmationFromQueueAsync(registrationId);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Registration confirmation email sent for registration {RegistrationId}", registrationId);
                }
                else
                {
                    _logger.LogWarning("Failed to send registration confirmation for {RegistrationId}: {Error}",
                        registrationId, result.ErrorMessage);
                    throw new InvalidOperationException(result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending registration confirmation email for {RegistrationId}", registrationId);
                throw;
            }
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task SendBibNotificationEmailAsync(int registrationId)
        {
            try
            {
                var result = await _emailQueueProcessor.SendBibNotificationFromQueueAsync(registrationId);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("BIB notification email sent for registration {RegistrationId}", registrationId);
                }
                else
                {
                    _logger.LogWarning("Failed to send BIB notification for {RegistrationId}: {Error}",
                        registrationId, result.ErrorMessage);
                    throw new InvalidOperationException(result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending BIB notification email for {RegistrationId}", registrationId);
                throw;
            }
        }

        public async Task SendPaymentReminderEmailsAsync(int raceId)
        {
            try
            {
                // Lấy danh sách registrations cần gửi reminder
                var registrations = await _unitOfWork.Registrations.GetPendingPaymentRegistrationsAsync(raceId);

                int successful = 0;
                int total = registrations.Count();

                foreach (var registration in registrations)
                {
                    var result = await _emailQueueProcessor.SendPaymentReminderFromQueueAsync(registration.Id);
                    if (result.IsSuccess)
                    {
                        successful++;
                    }
                }

                _logger.LogInformation("Payment reminder emails sent for race {RaceId}: {Successful}/{Total} successful",
                    raceId, successful, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment reminder emails for race {RaceId}", raceId);
                throw;
            }
        }

        public async Task SendRaceDayInfoEmailsAsync(int raceId)
        {
            try
            {
                // Lấy danh sách registrations đã paid
                var registrations = await _unitOfWork.Registrations.GetPaidRegistrationsAsync(raceId);

                int successful = 0;
                int total = registrations.Count();

                foreach (var registration in registrations)
                {
                    var result = await _emailQueueProcessor.SendRaceDayInfoFromQueueAsync(registration.Id);
                    if (result.IsSuccess)
                    {
                        successful++;
                    }
                }

                _logger.LogInformation("Race day info emails sent for race {RaceId}: {Successful}/{Total} successful",
                    raceId, successful, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending race day info emails for race {RaceId}", raceId);
                throw;
            }
        }

        public async Task CleanupOldEmailLogsAsync(int daysToKeep = 30)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);

                var oldLogs = await _unitOfWork.EmailLogs
                    .FindAsync(log => log.SentAt < cutoffDate && log.Status == EmailStatus.Sent);

                if (oldLogs.Any())
                {
                    foreach (var log in oldLogs)
                    {
                        _unitOfWork.EmailLogs.Remove(log);
                    }

                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("Cleaned up {Count} old email logs older than {Days} days",
                        oldLogs.Count(), daysToKeep);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old email logs");
                throw;
            }
        }
    }
}