using RaceManagement.Abstractions.Enums;
using RaceManagement.Core.Entities;
using RaceManagement.Core.Interfaces;
using RaceManagement.Shared.DTOs;
using System;
using System.Threading.Tasks;

namespace RaceManagement.Infrastructure.Services
{
    public class EmailQueueProcessor : IEmailQueueProcessor
    {
        private readonly IEmailService _emailService;

        public EmailQueueProcessor(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task<EmailResult> ProcessEmailFromQueueAsync(EmailQueue emailQueue)
        {
            if (emailQueue == null)
                throw new ArgumentNullException(nameof(emailQueue));

            return emailQueue.EmailType switch
            {
                EmailType.RegistrationConfirm => await SendRegistrationConfirmationFromQueueAsync(emailQueue.RegistrationId),
                EmailType.BibNumber => await SendBibNotificationFromQueueAsync(emailQueue.RegistrationId),
                EmailType.PaymentReminder => await SendPaymentReminderFromQueueAsync(emailQueue.RegistrationId),
                EmailType.RaceDayInfo => await SendRaceDayInfoFromQueueAsync(emailQueue.RegistrationId),
                _ => new EmailResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"EmailType {emailQueue.EmailType} not supported."
                }
            };
        }

        public async Task<EmailResult> SendRegistrationConfirmationFromQueueAsync(int registrationId)
        {
            return await _emailService.SendRegistrationConfirmationAsync(registrationId);
        }

        public async Task<EmailResult> SendBibNotificationFromQueueAsync(int registrationId)
        {
            return await _emailService.SendBibNotificationAsync(registrationId);
        }

        public async Task<EmailResult> SendPaymentReminderFromQueueAsync(int registrationId)
        {
            return await _emailService.SendPaymentReminderAsync(registrationId);
        }

        public async Task<EmailResult> SendRaceDayInfoFromQueueAsync(int registrationId)
        {
            return await _emailService.SendRaceDayInfoAsync(registrationId);
        }
    }
}
