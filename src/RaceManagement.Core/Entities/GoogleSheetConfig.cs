using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Core.Entities
{
    public class GoogleSheetConfig : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;              // "Sheet Marathon 2024"

        [Required]
        [MaxLength(255)]
        public string SpreadsheetId { get; set; } = string.Empty;     // Google Sheet ID

        [MaxLength(100)]
        public string? SheetName { get; set; }                        // Tên sheet cụ thể (optional)

        [MaxLength(255)]
        public string? Description { get; set; }                      // Mô tả

        public int CredentialId { get; set; }                         // FK to GoogleCredentials

        public int HeaderRowIndex { get; set; } = 1;                 // Row chứa header
        public int DataStartRowIndex { get; set; } = 2;              // Row bắt đầu data
        public int? LastSyncRowIndex { get; set; }                   // Row cuối cùng đã sync

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual GoogleCredential Credential { get; set; } = null!;
        public virtual ICollection<Race> Races { get; set; } = new List<Race>();

        // Helper methods
        public string GetSheetUrl()
        {
            var baseUrl = $"https://docs.google.com/spreadsheets/d/{SpreadsheetId}";
            return !string.IsNullOrEmpty(SheetName)
                ? $"{baseUrl}#gid={SheetName}"
                : baseUrl;
        }

        public string GetDisplayName() => $"{Name} ({Credential.Name})";

        public string GetCredentialPath() => Credential.GetAbsolutePath();

        /// <summary>
        /// Get range string for reading data
        /// </summary>
        public string GetDataRange(int? endRow = null)
        {
            var range = !string.IsNullOrEmpty(SheetName) ? $"'{SheetName}'!" : "";
            var startRow = DataStartRowIndex;
            var endRowStr = endRow?.ToString() ?? "1000";

            return $"{range}A{startRow}:Z{endRowStr}";
        }

        /// <summary>
        /// Get range string for reading header
        /// </summary>
        public string GetHeaderRange()
        {
            var range = !string.IsNullOrEmpty(SheetName) ? $"'{SheetName}'!" : "";
            return $"{range}A{HeaderRowIndex}:Z{HeaderRowIndex}";
        }

        /// <summary>
        /// Update last sync row and save
        /// </summary>
        public void UpdateLastSyncRow(int rowIndex)
        {
            LastSyncRowIndex = rowIndex;
            UpdatedAt = DateTime.Now;
        }
    }

}
