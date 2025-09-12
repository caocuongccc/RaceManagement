using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.DTOs
{
    public class RaceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime RaceDate { get; set; }
        public string Email { get; set; } = string.Empty;
        
        // Legacy field - keep for backward compatibility but will be phased out
        public string? SheetId { get; set; }

        // NEW - Sheet Config integration
        public int? SheetConfigId { get; set; }                    // NEW
        public string? SheetConfigName { get; set; }               // NEW  
        public string? CredentialName { get; set; }                // NEW


        public string? PaymentSheetId { get; set; }
        //public string? GoogleCredentialPath { get; set; }          // NEW
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<RaceDistanceDto> Distances { get; set; } = new();
        public List<RaceShirtTypeDto> ShirtTypes { get; set; } = new(); // NEW

        // Computed properties
        public bool IsUpcoming => RaceDate > DateTime.Now;
        public int DaysUntilRace => (RaceDate.Date - DateTime.Today).Days;
        public string TimeUntilRaceDisplay => IsUpcoming
            ? $"{DaysUntilRace} ngày"
            : $"Đã qua {Math.Abs(DaysUntilRace)} ngày";

        // NEW - Sheet config info
        public bool HasSheetConfig => SheetConfigId.HasValue;
        public string SheetSource => HasSheetConfig
            ? $"Sheet Config: {SheetConfigName} ({CredentialName})"
            : $"Legacy Sheet: {SheetId}";
    }

    public class RaceDistanceDto
    {
        public int Id { get; set; }
        public string Distance { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? BibPrefix { get; set; }
        public int? MaxParticipants { get; set; }
        public string PriceDisplay => $"{Price:N0} VNĐ";
    }

    public class RaceShirtTypeDto                                  // NEW
    {
        public int Id { get; set; }
        public string ShirtCategory { get; set; } = string.Empty;
        public string ShirtType { get; set; } = string.Empty;
        public string AvailableSizes { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public bool IsActive { get; set; }
        public List<string> SizesList => AvailableSizes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                       .Select(s => s.Trim())
                                                       .ToList();
        public string DisplayName => $"{ShirtCategory} {ShirtType}";
        public string PriceDisplay => Price.HasValue ? $"{Price:N0} VNĐ" : "Miễn phí";
    }
}
