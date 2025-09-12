using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.DTOs
{
    public class SheetRegistrationDto
    {
        public int RowIndex { get; set; }
        public DateTime Timestamp { get; set; }                     // A - Timestamp
        public string Email { get; set; } = string.Empty;          // B - Email
        public string FullName { get; set; } = string.Empty;       // D - Tên VĐV
        public string BibName { get; set; } = string.Empty;        // E - Tên trên BIB (NEW)
        public string RawBirthInput { get; set; } = string.Empty;  // F - Ngày sinh raw (NEW)
        public DateTime? DateOfBirth { get; set; }                 // F - Parsed date (NEW)
        public int? BirthYear { get; set; }                        // Extracted year
        public string Phone { get; set; } = string.Empty;          // G - Phone
        public string Distance { get; set; } = string.Empty;       // H - Distance
        public string? Gender { get; set; }                        // I - Gender
        public string? ShirtCategory { get; set; }                 // L - Shirt Category (NEW)
        public string? ShirtSize { get; set; }                     // M - Shirt Size
        public string? ShirtType { get; set; }                     // N - Shirt Type (NEW)
        public string? EmergencyContact { get; set; }
    }
}
