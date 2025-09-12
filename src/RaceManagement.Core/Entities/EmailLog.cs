using RaceManagement.Abstractions.Enums;
using System.ComponentModel.DataAnnotations;

namespace RaceManagement.Core.Entities
{
    public class EmailLog : BaseEntity
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

        public EmailType EmailType { get; set; }
        public EmailStatus Status { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;
        public TimeSpan ProcessingTime { get; set; }

        [MaxLength(255)]
        public string? MessageId { get; set; }

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        public int RetryCount { get; set; } = 0;

        // Template and content info
        [MaxLength(100)]
        public string? TemplateName { get; set; }

        public int? EmailQueueId { get; set; }

        // Navigation properties
        public virtual Registration Registration { get; set; } = null!;
        public virtual EmailQueue? EmailQueue { get; set; }

        // Helper methods
        public bool IsSuccess => Status == EmailStatus.Sent;

        public string GetStatusDisplay()
        {
            return Status switch
            {
                EmailStatus.Sent => "✅ Đã gửi",
                EmailStatus.Failed => $"❌ Thất bại ({RetryCount} lần thử)",
                EmailStatus.Processing => "🔄 Đang xử lý",
                EmailStatus.Pending => "⏳ Chờ gửi",
                EmailStatus.Cancelled => "⏹️ Đã hủy",
                _ => "❓ Không xác định"
            };
        }
    }
}
