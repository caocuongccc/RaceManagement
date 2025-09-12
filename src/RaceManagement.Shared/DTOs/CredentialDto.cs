
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RaceManagement.Shared.DTOs
{
    public class CredentialDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ServiceAccountEmail { get; set; } = string.Empty;
        public string CredentialFilePath { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Computed properties
        public int SheetConfigCount { get; set; }
        public bool FileExists { get; set; }
        public string FileName => Path.GetFileName(CredentialFilePath);
        public string Status => IsActive ? (FileExists ? "Hoạt động" : "File không tồn tại") : "Không hoạt động";
    }

    public class CreateCredentialDto
    {
        [Required(ErrorMessage = "Tên credential là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "File credential JSON là bắt buộc")]
        public IFormFile CredentialFile { get; set; } = null!;

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        // Server sẽ tự động parse ServiceAccountEmail từ file JSON
    }

    public class UpdateCredentialDto
    {
        [Required(ErrorMessage = "Tên credential là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Optional - nếu muốn update file credential
        public IFormFile? CredentialFile { get; set; }
    }

    public class CredentialTestResult
    {
        public int CredentialId { get; set; }
        public string CredentialName { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public bool FileExists { get; set; }
        public bool CanAuthenticate { get; set; }
        public List<string> Errors { get; set; } = new();
        public ServiceAccountInfoDto? ServiceAccountInfo { get; set; }
        public DateTime TestedAt { get; set; } = DateTime.Now;

        public string GetStatusMessage()
        {
            if (IsValid && CanAuthenticate)
                return "✅ Credential hợp lệ và có thể kết nối";

            if (!FileExists)
                return "❌ File credential không tồn tại";

            if (!IsValid)
                return $"❌ File không hợp lệ: {string.Join(", ", Errors)}";

            if (!CanAuthenticate)
                return "⚠️ File hợp lệ nhưng không thể xác thực";

            return "❓ Trạng thái không xác định";
        }
    }

    public class ServiceAccountInfoDto
    {
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string PrivateKeyId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
