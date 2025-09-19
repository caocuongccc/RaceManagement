using RaceManagement.Shared.Enums;
using RaceManagement.Core.Entities;
using RaceManagement.Shared.DTOs;

namespace RaceManagement.Core.Interfaces
{
    // Core email sender - không phụ thuộc vào Job
    public interface IEmailSender
    {
        Task<EmailResult> SendEmailAsync(EmailRequest request);
        Task<byte[]> GenerateQRCodeAsync(string content);
        Task<byte[]> GeneratePaymentQRCodeAsync(RegistrationDto registration);
    }
    // Email business logic - sử dụng IEmailSender thay vì IEmailJob
    public interface IEmailService
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

        //// Wrapper methods cho Hangfire (không async)
        void SendRegistrationConfirmationEmail(int registrationId);
        void SendBibNotificationEmail(int registrationId);
        void SendPaymentReminderEmails(int raceId);
        void SendRaceDayInfoEmails(int raceId);
        void ProcessPendingEmails();
    }

    // Queue processor - chỉ xử lý queue, không gọi lại EmailService
    public interface IEmailQueueProcessor
    {
        Task<EmailResult> ProcessEmailFromQueueAsync(EmailQueue emailQueue);
        Task<EmailResult> SendRegistrationConfirmationFromQueueAsync(int registrationId);
        Task<EmailResult> SendBibNotificationFromQueueAsync(int registrationId);
        Task<EmailResult> SendPaymentReminderFromQueueAsync(int registrationId);
        Task<EmailResult> SendRaceDayInfoFromQueueAsync(int registrationId);
    }

    // Template service - giữ nguyên như cũ
    public interface IEmailTemplateService
    {
        Task<string> RenderTemplateAsync(string templateName, object model);
        Task<bool> TemplateExistsAsync(string templateName);
        Task<IEnumerable<string>> GetAvailableTemplatesAsync();
        string GetTemplateSubject(string templateName, object model);
    }
}
