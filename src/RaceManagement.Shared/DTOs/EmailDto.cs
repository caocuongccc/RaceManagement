using RaceManagement.Shared.Enums;
using EmailPriority = RaceManagement.Shared.Enums.EmailPriority;

namespace RaceManagement.Shared.DTOs
{
    public class EmailRequest
    {
        public string To { get; set; } = string.Empty;
        public string? ToName { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
        public string? PlainTextContent { get; set; }
        public string? HtmlContent { get; set; }
        public List<EmailAttachment> Attachments { get; set; } = new();
        public Dictionary<string, string> Headers { get; set; } = new();
        public EmailPriority Priority { get; set; } = EmailPriority.Normal;
        public int? RegistrationId { get; set; }
        public EmailType? EmailType { get; set; }

    }

    public class EmailAttachment
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "application/octet-stream";
        public bool IsInline { get; set; } = false;
        public string? ContentId { get; set; }
    }

    public class EmailResult
    {
        public bool IsSuccess { get; set; }
        public string? MessageId { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
        public TimeSpan ProcessingTime { get; set; }
        public int? EmailLogId { get; set; }
    }

    public class BulkEmailRequest
    {
        public List<EmailRequest> Emails { get; set; } = new();
        public bool StopOnFirstError { get; set; } = false;
        public int BatchSize { get; set; } = 10;
        public int DelayBetweenBatches { get; set; } = 5000;
    }

    public class BulkEmailResult
    {
        public int TotalEmails { get; set; }
        public int SuccessfulEmails { get; set; }
        public int FailedEmails { get; set; }
        public List<EmailResult> Results { get; set; } = new();
        public TimeSpan TotalProcessingTime { get; set; }
        public double SuccessRate => TotalEmails > 0 ? (double)SuccessfulEmails / TotalEmails * 100 : 0;
    }

    public class EmailQueueStatusDto
    {
        public int PendingEmails { get; set; }
        public int ProcessingEmails { get; set; }
        public int CompletedEmails { get; set; }
        public int FailedEmails { get; set; }
        public DateTime? NextScheduledEmail { get; set; }
        public List<EmailQueueItemDto> RecentItems { get; set; } = new();
    }

    public class EmailQueueItemDto
    {
        public int Id { get; set; }
        public int RegistrationId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public EmailType EmailType { get; set; }
        public EmailStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public int RetryCount { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
