using RaceManagement.Abstractions.Enums;
using System.ComponentModel.DataAnnotations;

namespace RaceManagement.Core.Entities
{
    public class EmailQueue : BaseEntity
    {
        public int RegistrationId { get; set; }

        [Required]
        [MaxLength(255)]
        public string RecipientEmail { get; set; } = string.Empty;

        [MaxLength(255)]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Subject { get; set; } = string.Empty;

        public string? PlainTextContent { get; set; }
        public string? HtmlContent { get; set; }

        public EmailType EmailType { get; set; }
        public EmailPriority Priority { get; set; } = EmailPriority.Normal;
        public EmailStatus Status { get; set; } = EmailStatus.Pending;

        public DateTime? ScheduledAt { get; set; }
        public DateTime? ProcessedAt { get; set; }

        public int RetryCount { get; set; } = 0;
        public int MaxRetries { get; set; } = 3;

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        [MaxLength(255)]
        public string? MessageId { get; set; }

        // JSON field for attachments and metadata
        [MaxLength(4000)]
        public string? Metadata { get; set; }

        // Navigation properties
        public virtual Registration Registration { get; set; } = null!;

        // Helper methods
        public bool CanRetry => RetryCount < MaxRetries && Status == EmailStatus.Failed;

        public bool IsScheduled => ScheduledAt.HasValue && ScheduledAt > DateTime.Now;

        public void IncrementRetry(string errorMessage)
        {
            RetryCount++;
            ErrorMessage = errorMessage;
            Status = CanRetry ? EmailStatus.Pending : EmailStatus.Failed;
            UpdatedAt = DateTime.Now;
        }

        public void MarkAsProcessing()
        {
            Status = EmailStatus.Processing;
            ProcessedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        public void MarkAsSent(string? messageId = null)
        {
            Status = EmailStatus.Sent;
            MessageId = messageId;
            ProcessedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        public void MarkAsFailed(string errorMessage)
        {
            Status = EmailStatus.Failed;
            ErrorMessage = errorMessage;
            ProcessedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }
    }
}
