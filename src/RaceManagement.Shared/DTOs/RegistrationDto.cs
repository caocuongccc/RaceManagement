namespace RaceManagement.Shared.DTOs
{
    public class RegistrationDto
    {
        public int Id { get; set; }
        public int RaceId { get; set; }
        public string RaceName { get; set; } = string.Empty;

        public int DistanceId { get; set; }
        public string Distance { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string BibName { get; set; } = string.Empty; // tên in trên bib

        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }

        public int? BirthYear { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? RawBirthInput { get; set; }

        public string? Gender { get; set; }

        // Áo
        public string? ShirtCategory { get; set; }
        public string? ShirtSize { get; set; }
        public string? ShirtType { get; set; }
        public string ShirtFullDescription =>
            string.Join(" - ", new[] { ShirtCategory, ShirtType, $"Size {ShirtSize}" }
                .Where(s => !string.IsNullOrEmpty(s)));

        // Liên hệ khẩn cấp
        public string? EmergencyContact { get; set; }

        // Thanh toán
        public string PaymentStatus { get; set; } = string.Empty; // "Paid", "Unpaid", "Pending"
        public bool IsPaid => PaymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase);

        public string TransactionReference { get; set; } = string.Empty;

        // Bib
        public string? BibNumber { get; set; }
        public DateTime? BibSentAt { get; set; }

        // Khác
        public DateTime RegistrationTime { get; set; }

        // Computed
        public int Age
        {
            get
            {
                if (DateOfBirth.HasValue)
                {
                    var today = DateTime.Today;
                    var age = today.Year - DateOfBirth.Value.Year;
                    if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
                    return age;
                }
                if (BirthYear.HasValue) return DateTime.Today.Year - BirthYear.Value;
                return 0;
            }
        }
    }
}
