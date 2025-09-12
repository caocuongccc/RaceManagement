
using Microsoft.AspNetCore.Http;

namespace RaceManagement.Core.Helpers
{
    public static class FileUploadHelper
    {
        private static readonly string[] AllowedExtensions = { ".json" };
        private const long MaxFileSizeBytes = 1024 * 1024; // 1MB

        public static FileValidationResult ValidateCredentialFile(IFormFile file)
        {
            var result = new FileValidationResult();

            if (file == null || file.Length == 0)
            {
                result.AddError("File is required");
                return result;
            }

            // Check file size
            if (file.Length > MaxFileSizeBytes)
            {
                result.AddError($"File size exceeds {MaxFileSizeBytes / (1024 * 1024)}MB limit");
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                result.AddError($"Only {string.Join(", ", AllowedExtensions)} files are allowed");
            }

            // Check content type
            if (file.ContentType != "application/json" &&
                file.ContentType != "application/octet-stream")
            {
                result.AddError("Invalid content type. Expected JSON file");
            }

            return result;
        }

        public static string GenerateUniqueFileName(string originalFileName, string credentialName)
        {
            var extension = Path.GetExtension(originalFileName);
            var safeName = string.Join("", credentialName.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'));
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

            return $"{safeName}-{timestamp}{extension}";
        }

        public static async Task<string> SaveCredentialFileAsync(IFormFile file, string credentialName, string baseDirectory = "credentials")
        {
            // Create directory if not exists
            var targetDirectory = Path.Combine(Directory.GetCurrentDirectory(), baseDirectory, credentialName);
            Directory.CreateDirectory(targetDirectory);

            // Generate unique filename
            var fileName = GenerateUniqueFileName(file.FileName, credentialName);
            var filePath = Path.Combine(targetDirectory, fileName);

            // Save file
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Return relative path
            return Path.Combine(baseDirectory, credentialName, fileName);
        }
    }

    public class FileValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; set; } = new();

        public void AddError(string error)
        {
            Errors.Add(error);
        }
    }
}
