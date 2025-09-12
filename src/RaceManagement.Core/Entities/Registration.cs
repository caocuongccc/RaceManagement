using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RaceManagement.Abstractions.Enums;

namespace RaceManagement.Core.Entities
{
    public class Registration : BaseEntity
    {
        public int RaceId { get; set; }
        public int DistanceId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string BibName { get; set; } = string.Empty;           // NEW: Tên trên BIB
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }

        // Birth date handling - Enhanced
        public int? BirthYear { get; set; }
        public DateTime? DateOfBirth { get; set; }                    // NEW: Ngày sinh chuẩn hóa
        public string? RawBirthInput { get; set; }                   // NEW: Input gốc từ sheet

        public Gender? Gender { get; set; }

        // Shirt management - Enhanced
        public string? ShirtCategory { get; set; }                   // NEW: Nam/Nữ/Trẻ em
        public string? ShirtSize { get; set; }
        public string? ShirtType { get; set; }                       // NEW: T-Shirt/Singlet

        public string? EmergencyContact { get; set; }
        public DateTime RegistrationTime { get; set; } = DateTime.Now;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public string? BibNumber { get; set; }
        public DateTime? BibSentAt { get; set; }
        public string? QRCodePath { get; set; }
        public int? SheetRowIndex { get; set; }
        public string TransactionReference { get; set; } = string.Empty;

        // Navigation properties
        public virtual Race Race { get; set; } = null!;
        public virtual RaceDistance Distance { get; set; } = null!;
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<EmailLog> EmailLogs { get; set; } = new List<EmailLog>();

        // Helper methods
        public int CalculateAge(DateTime? referenceDate = null)
        {
            if (!DateOfBirth.HasValue) return 0;

            var reference = referenceDate ?? DateTime.Today;
            var age = reference.Year - DateOfBirth.Value.Year;

            if (DateOfBirth.Value.Date > reference.AddYears(-age))
                age--;

            return age;
        }

        public string GetShirtFullDescription()
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(ShirtCategory)) parts.Add(ShirtCategory);
            if (!string.IsNullOrEmpty(ShirtType)) parts.Add(ShirtType);
            if (!string.IsNullOrEmpty(ShirtSize)) parts.Add($"Size {ShirtSize}");

            return string.Join(" - ", parts);
        }
    }
}
