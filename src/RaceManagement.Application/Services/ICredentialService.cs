using Microsoft.AspNetCore.Http;
using RaceManagement.Core.Models;
using RaceManagement.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Application.Services
{
    public interface ICredentialService
    {
        // CRUD Operations
        Task<CredentialDto> CreateCredentialAsync(CreateCredentialDto dto);
        Task<CredentialDto> UpdateCredentialAsync(int id, UpdateCredentialDto dto);
        Task DeleteCredentialAsync(int id);
        Task<CredentialDto?> GetCredentialAsync(int id);
        Task<IEnumerable<CredentialDto>> GetAllCredentialsAsync();
        Task<IEnumerable<CredentialDto>> GetActiveCredentialsAsync();

        // Testing & Validation
        Task<CredentialTestResult> TestCredentialAsync(int id);
        Task<CredentialValidationResult> ValidateCredentialFileAsync(IFormFile file);

        // File Management
        Task<string> UploadCredentialFileAsync(IFormFile file, string credentialName);
        Task<bool> DeleteCredentialFileAsync(string filePath);

        // Search & Selection
        Task<IEnumerable<CredentialDto>> SearchCredentialsAsync(string searchTerm);
        Task<IEnumerable<CredentialSelectDto>> GetCredentialSelectListAsync();

        // Bulk Operations
        Task<BulkOperationResult> BulkUpdateStatusAsync(BulkCredentialOperation operation);
    }

}
