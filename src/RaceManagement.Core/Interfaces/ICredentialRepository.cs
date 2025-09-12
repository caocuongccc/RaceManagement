using RaceManagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Core.Interfaces
{
    public interface ICredentialRepository : IRepository<GoogleCredential>
    {
        Task<IEnumerable<GoogleCredential>> GetActiveCredentialsAsync();
        Task<IEnumerable<GoogleCredential>> GetCredentialsWithSheetConfigsAsync();
        Task<GoogleCredential?> GetByNameAsync(string name);
        Task<GoogleCredential?> GetByEmailAsync(string email);
        Task<GoogleCredential?> GetWithSheetConfigsAsync(int id);
        Task<bool> IsNameExistsAsync(string name, int? excludeId = null);
        Task<bool> IsEmailExistsAsync(string email, int? excludeId = null);
        Task<int> GetSheetConfigCountAsync(int credentialId);
        Task<IEnumerable<GoogleCredential>> SearchAsync(string searchTerm);
    }

    public interface ISheetConfigRepository : IRepository<GoogleSheetConfig>
    {
        Task<IEnumerable<GoogleSheetConfig>> GetActiveConfigsAsync();
        Task<IEnumerable<GoogleSheetConfig>> GetConfigsWithCredentialsAsync();
        Task<IEnumerable<GoogleSheetConfig>> GetConfigsByCredentialAsync(int credentialId);
        Task<GoogleSheetConfig?> GetConfigWithCredentialAsync(int configId);
        Task<GoogleSheetConfig?> GetBySpreadsheetIdAsync(string spreadsheetId);
        Task<bool> IsNameExistsAsync(string name, int credentialId, int? excludeId = null);
        Task<bool> IsSpreadsheetIdExistsAsync(string spreadsheetId, int? excludeId = null);
        Task<int> GetRaceCountAsync(int sheetConfigId);
        Task UpdateLastSyncRowAsync(int configId, int rowIndex);
        Task<IEnumerable<GoogleSheetConfig>> SearchAsync(string searchTerm);
    }
}
