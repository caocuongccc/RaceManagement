using RaceManagement.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Application.Services
{
    public interface ISheetConfigService
    {
        // CRUD Operations
        Task<SheetConfigDto> CreateSheetConfigAsync(CreateSheetConfigDto dto);
        Task<SheetConfigDto> UpdateSheetConfigAsync(int id, UpdateSheetConfigDto dto);
        Task DeleteSheetConfigAsync(int id);
        Task<SheetConfigDto?> GetSheetConfigAsync(int id);
        Task<IEnumerable<SheetConfigDto>> GetAllSheetConfigsAsync();
        Task<IEnumerable<SheetConfigDto>> GetActiveSheetConfigsAsync();
        Task<IEnumerable<SheetConfigDto>> GetConfigsByCredentialAsync(int credentialId);

        // Testing & Validation
        Task<SheetConfigTestResult> TestSheetConnectionAsync(int id);
        Task<SheetMetadataDto> GetSheetMetadataAsync(int id);
        Task<IEnumerable<string>> GetSheetNamesAsync(int id);

        // Search & Selection
        Task<IEnumerable<SheetConfigDto>> SearchSheetConfigsAsync(string searchTerm);
        Task<IEnumerable<SheetConfigSelectDto>> GetSheetConfigSelectListAsync();
        Task<IEnumerable<SheetConfigSelectDto>> GetSheetConfigSelectByCredentialAsync(int credentialId);

        // Sync Management
        Task UpdateLastSyncRowAsync(int configId, int rowIndex);
        Task<int> GetNextSyncRowAsync(int configId);

        // Bulk Operations
        Task<BulkOperationResult> BulkUpdateStatusAsync(BulkSheetConfigOperation operation);
    }
}
