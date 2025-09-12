using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.DTOs
{
    public class SheetConfigDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SpreadsheetId { get; set; } = string.Empty;
        public string? SheetName { get; set; }
        public string? Description { get; set; }
        public int CredentialId { get; set; }
        public string CredentialName { get; set; } = string.Empty;
        public int HeaderRowIndex { get; set; }
        public int DataStartRowIndex { get; set; }
        public int? LastSyncRowIndex { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Computed properties
        public int RaceCount { get; set; }
        public string SheetUrl { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Status => IsActive ? "Hoạt động" : "Không hoạt động";
        public string LastSyncInfo => LastSyncRowIndex.HasValue
            ? $"Dòng {LastSyncRowIndex}"
            : "Chưa sync";
    }

    public class CreateSheetConfigDto
    {
        [Required(ErrorMessage = "Tên sheet config là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Spreadsheet ID là bắt buộc")]
        [MaxLength(255, ErrorMessage = "Spreadsheet ID không được vượt quá 255 ký tự")]
        public string SpreadsheetId { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Tên sheet không được vượt quá 100 ký tự")]
        public string? SheetName { get; set; }

        [MaxLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Credential là bắt buộc")]
        public int CredentialId { get; set; }

        [Range(1, 100, ErrorMessage = "Header row index phải từ 1 đến 100")]
        public int HeaderRowIndex { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Data start row index phải từ 1 đến 100")]
        public int DataStartRowIndex { get; set; } = 2;
    }

    public class UpdateSheetConfigDto
    {
        [Required(ErrorMessage = "Tên sheet config là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Tên sheet không được vượt quá 100 ký tự")]
        public string? SheetName { get; set; }

        [MaxLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự")]
        public string? Description { get; set; }

        [Range(1, 100, ErrorMessage = "Header row index phải từ 1 đến 100")]
        public int HeaderRowIndex { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Data start row index phải từ 1 đến 100")]
        public int DataStartRowIndex { get; set; } = 2;

        public bool IsActive { get; set; } = true;
    }

    public class SheetConfigTestResult
    {
        public int SheetConfigId { get; set; }
        public string SheetConfigName { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
        public bool CanReadData { get; set; }
        public int? TotalRows { get; set; }
        public int? DataRows { get; set; }
        public List<string> Errors { get; set; } = new();
        public SheetMetadataDto? Metadata { get; set; }
        public DateTime TestedAt { get; set; } = DateTime.Now;

        public string GetStatusMessage()
        {
            if (IsConnected && CanReadData)
                return $"✅ Kết nối thành công - {DataRows} dòng dữ liệu";

            if (!IsConnected)
                return $"❌ Không thể kết nối: {string.Join(", ", Errors)}";

            if (!CanReadData)
                return "⚠️ Kết nối được nhưng không đọc được dữ liệu";

            return "❓ Trạng thái không xác định";
        }
    }
}
