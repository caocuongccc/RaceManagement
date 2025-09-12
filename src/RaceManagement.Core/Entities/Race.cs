using RaceManagement.Core.Entities;
using RaceManagement.Abstractions.Enums;


namespace RaceManagement.Core.Entities
{
    public class Race : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public DateTime RaceDate { get; set; }
        public string Email { get; set; } = string.Empty;
        public string EmailPassword { get; set; } = string.Empty;
        public string SheetId { get; set; } = string.Empty;
        public string? PaymentSheetId { get; set; }
        public string? GoogleCredentialPath { get; set; }             // NEW: Specific credential file
        public RaceStatus Status { get; set; } = RaceStatus.Active;

        // Navigation properties
        public virtual ICollection<RaceDistance> Distances { get; set; } = new List<RaceDistance>();
        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
        public virtual ICollection<RaceShirtType> ShirtTypes { get; set; } = new List<RaceShirtType>(); // NEW

        // Helper methods
        public bool IsActive => Status == RaceStatus.Active;

        public bool IsUpcoming => RaceDate > DateTime.Now;

        public TimeSpan TimeUntilRace => RaceDate - DateTime.Now;

        public string GetCredentialPath(string basePath)
        {
            return string.IsNullOrEmpty(GoogleCredentialPath)
                ? Path.Combine(basePath, "default", "google-service-account.json")
                : GoogleCredentialPath;
        }

        public RaceShirtType? FindShirtType(string category, string type)
        {
            return ShirtTypes.FirstOrDefault(st =>
                st.ShirtCategory.Equals(category, StringComparison.OrdinalIgnoreCase) &&
                st.ShirtType.Equals(type, StringComparison.OrdinalIgnoreCase) &&
                st.IsActive);
        }

        public List<string> GetAvailableShirtCategories()
        {
            return ShirtTypes.Where(st => st.IsActive)
                            .Select(st => st.ShirtCategory)
                            .Distinct()
                            .ToList();
        }

        // Add these properties to existing Race entity:
        public int? SheetConfigId { get; set; }                          // NEW - FK to GoogleSheetConfig

        // Add to navigation properties:
        public virtual GoogleSheetConfig? SheetConfig { get; set; }       // NEW - Navigation property

        // Add these helper methods to existing Race entity:
        /// <summary>
        /// Get Google Sheet ID from config or fallback to legacy SheetId
        /// </summary>
        public string GetGoogleSheetId()
        {
            return SheetConfig?.SpreadsheetId ?? SheetId ?? string.Empty;
        }

        /// <summary>
        /// Get credential file path from config
        /// </summary>
        public string GetCredentialPath()
        {
            return SheetConfig?.GetCredentialPath() ?? string.Empty;
        }

        /// <summary>
        /// Get sheet name for multi-sheet spreadsheets
        /// </summary>
        public string? GetSheetName()
        {
            return SheetConfig?.SheetName;
        }

        /// <summary>
        /// Get data range for reading registrations
        /// </summary>
        public string GetDataRange(int? endRow = null)
        {
            if (SheetConfig != null)
            {
                return SheetConfig.GetDataRange(endRow);
            }

            // Fallback for legacy races
            var startRow = 2; // Default start row
            var endRowStr = endRow?.ToString() ?? "1000";
            return $"A{startRow}:Z{endRowStr}";
        }

        /// <summary>
        /// Check if race has sheet configuration
        /// </summary>
        public bool HasSheetConfig => SheetConfig != null && SheetConfig.IsActive;

    }
}


