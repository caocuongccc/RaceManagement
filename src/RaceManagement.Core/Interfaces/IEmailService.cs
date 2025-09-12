using RaceManagement.Abstractions.Enums;
using RaceManagement.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Core.Interfaces
{
    public interface IEmailServices
    {

        // Core email sending
        Task<EmailResult> SendEmailAsync(EmailRequest request);
        Task<EmailResult> SendRegistrationConfirmationAsync(int registrationId);
        Task<EmailResult> SendBibNotificationAsync(int registrationId);
        Task<EmailResult> SendPaymentReminderAsync(int registrationId);
        Task<EmailResult> SendRaceDayInfoAsync(int registrationId);

        // Bulk email operations
        Task<BulkEmailResult> SendBulkEmailAsync(BulkEmailRequest request);
        Task<BulkEmailResult> SendRaceNotificationsAsync(int raceId, EmailType emailType);

        // Template management
        Task<string> RenderTemplateAsync(string templateName, object model);
        Task<bool> ValidateTemplateAsync(string templateName);

        // QR Code generation
        Task<byte[]> GenerateQRCodeAsync(string content);
        Task<byte[]> GeneratePaymentQRCodeAsync(RegistrationDto registration);

        // Email queue management
        Task QueueEmailAsync(int registrationId, EmailType emailType, DateTime? scheduledAt = null);
        Task<EmailQueueStatusDto> GetQueueStatusAsync();

        // Wrapper methods cho Hangfire (không async)
    void SendRegistrationConfirmationEmail(int registrationId);
    void SendBibNotificationEmail(int registrationId);
    void SendPaymentReminderEmails(int raceId);
    void SendRaceDayInfoEmails(int raceId);
    void ProcessPendingEmails();
    }
}
