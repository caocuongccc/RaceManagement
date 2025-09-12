using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RaceManagement.Core.Models;

namespace RaceManagement.Core.Entities
{
    public class GoogleCredential : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;              // "BTC Phường A"

        [MaxLength(255)]
        public string? Description { get; set; }                      // Mô tả thêm

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string ServiceAccountEmail { get; set; } = string.Empty; // Email từ JSON

        [Required]
        [MaxLength(500)]
        public string CredentialFilePath { get; set; } = string.Empty;  // Path to JSON file

        public bool IsActive { get; set; } = true;

        [MaxLength(100)]
        public string? CreatedBy { get; set; }                        // User tạo credential

        // Navigation properties
        public virtual ICollection<GoogleSheetConfig> SheetConfigs { get; set; } = new List<GoogleSheetConfig>();

        // Helper methods
        public string GetFileName() => Path.GetFileName(CredentialFilePath);

        public bool FileExists() => File.Exists(GetAbsolutePath());

        public string GetAbsolutePath()
        {
            return Path.IsPathRooted(CredentialFilePath)
                ? CredentialFilePath
                : Path.Combine(Directory.GetCurrentDirectory(), CredentialFilePath);
        }

        /// <summary>
        /// Parse service account info from JSON file
        /// </summary>
        public async Task<ServiceAccountInfo?> GetServiceAccountInfoAsync()
        {
            try
            {
                if (!FileExists()) return null;

                var jsonContent = await File.ReadAllTextAsync(GetAbsolutePath());
                var jsonDoc = JsonDocument.Parse(jsonContent);
                var root = jsonDoc.RootElement;

                return new ServiceAccountInfo
                {
                    ClientEmail = root.GetProperty("client_email").GetString() ?? string.Empty,
                    ClientId = root.GetProperty("client_id").GetString() ?? string.Empty,
                    ProjectId = root.GetProperty("project_id").GetString() ?? string.Empty,
                    PrivateKeyId = root.GetProperty("private_key_id").GetString() ?? string.Empty,
                    Type = root.GetProperty("type").GetString() ?? string.Empty
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Validate credential file format
        /// </summary>
        public async Task<CredentialValidationResult> ValidateAsync()
        {
            var result = new CredentialValidationResult();

            // Check file exists
            if (!FileExists())
            {
                result.AddError("Credential file not found");
                return result;
            }

            try
            {
                // Parse JSON
                var info = await GetServiceAccountInfoAsync();
                if (info == null)
                {
                    result.AddError("Invalid JSON format");
                    return result;
                }

                // Validate required fields
                if (string.IsNullOrEmpty(info.ClientEmail))
                    result.AddError("Missing client_email in JSON");

                if (string.IsNullOrEmpty(info.ProjectId))
                    result.AddError("Missing project_id in JSON");

                if (string.IsNullOrEmpty(info.PrivateKeyId))
                    result.AddError("Missing private_key_id in JSON");

                if (info.Type != "service_account")
                    result.AddError("Invalid type, must be 'service_account'");

                // Check email matches
                if (!string.IsNullOrEmpty(info.ClientEmail) &&
                    !info.ClientEmail.Equals(ServiceAccountEmail, StringComparison.OrdinalIgnoreCase))
                {
                    result.AddError($"Service account email mismatch. Expected: {ServiceAccountEmail}, Found: {info.ClientEmail}");
                }

                result.ServiceAccountInfo = info;
            }
            catch (Exception ex)
            {
                result.AddError($"JSON parsing error: {ex.Message}");
            }

            return result;
        }
    }
}
