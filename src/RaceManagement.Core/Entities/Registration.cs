using RaceManagement.Shared.Enums;
using System.ComponentModel.DataAnnotations.Schema;

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
        public decimal Fee { get; set; }   // Distance.Price + Shirt.Price (nếu có)

        // Navigation properties
        public virtual Race Race { get; set; } = null!;
        public virtual RaceDistance Distance { get; set; } = null!;
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<EmailLog> EmailLogs { get; set; } = new List<EmailLog>();

        // Helper methods

        [NotMapped]
        public decimal TotalAmount
        {
            get
            {
                decimal? total = Distance?.Price ?? 0;

                if (Race?.HasShirtSale == true && !string.IsNullOrEmpty(ShirtSize))
                {
                    // Tìm đúng loại áo mà VĐV chọn
                    var shirt = Race.ShirtTypes
                        .FirstOrDefault(st =>
                            st.IsActive &&
                            st.ShirtCategory.Equals(ShirtCategory, StringComparison.OrdinalIgnoreCase) &&
                            st.ShirtType.Equals(ShirtType, StringComparison.OrdinalIgnoreCase));

                    if (shirt != null)
                        total += shirt.Price;
                }

                return (decimal)total;
            }
        }

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
