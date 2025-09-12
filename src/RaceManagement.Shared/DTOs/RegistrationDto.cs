using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string BibName { get; set; } = string.Empty;        // NEW
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int? BirthYear { get; set; }
        public DateTime? DateOfBirth { get; set; }                 // NEW
        public string? RawBirthInput { get; set; }                 // NEW
        public string? Gender { get; set; }
        public string? ShirtCategory { get; set; }                 // NEW
        public string? ShirtSize { get; set; }
        public string? ShirtType { get; set; }                     // NEW
        public string? EmergencyContact { get; set; }
        public DateTime RegistrationTime { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string? BibNumber { get; set; }
        public DateTime? BibSentAt { get; set; }
        public string TransactionReference { get; set; } = string.Empty;

        // Computed properties
        public int Age => DateOfBirth.HasValue
            ? DateTime.Today.Year - DateOfBirth.Value.Year - (DateTime.Today.DayOfYear < DateOfBirth.Value.DayOfYear ? 1 : 0)
            : 0;

        public string ShirtFullDescription => string.Join(" - ",
            new[] { ShirtCategory, ShirtType, $"Size {ShirtSize}" }.Where(s => !string.IsNullOrEmpty(s)));
    }

}
