using Microsoft.EntityFrameworkCore;
using RaceManagement.Core.Entities;
using RaceManagement.Core.Interfaces;
using RaceManagement.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Infrastructure.Repositories
{
    public class SheetConfigRepository : Repository<GoogleSheetConfig>, ISheetConfigRepository
    {
        public SheetConfigRepository(RaceManagementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<GoogleSheetConfig>> GetActiveConfigsAsync()
        {
            return await _dbSet
                .Where(sc => sc.IsActive)
                .Include(sc => sc.Credential)
                .Include(sc => sc.Races)
                .OrderBy(sc => sc.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<GoogleSheetConfig>> GetConfigsWithCredentialsAsync()
        {
            return await _dbSet
                .Include(sc => sc.Credential)
                .Include(sc => sc.Races)
                .OrderByDescending(sc => sc.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<GoogleSheetConfig>> GetConfigsByCredentialAsync(int credentialId)
        {
            return await _dbSet
                .Where(sc => sc.CredentialId == credentialId)
                .Include(sc => sc.Credential)
                .Include(sc => sc.Races)
                .OrderBy(sc => sc.Name)
                .ToListAsync();
        }

        public async Task<GoogleSheetConfig?> GetConfigWithCredentialAsync(int configId)
        {
            return await _dbSet
                .Include(sc => sc.Credential)
                .Include(sc => sc.Races)
                .FirstOrDefaultAsync(sc => sc.Id == configId);
        }

        public async Task<GoogleSheetConfig?> GetBySpreadsheetIdAsync(string spreadsheetId)
        {
            return await _dbSet
                .Include(sc => sc.Credential)
                .FirstOrDefaultAsync(sc => sc.SpreadsheetId == spreadsheetId);
        }

        public async Task<bool> IsNameExistsAsync(string name, int credentialId, int? excludeId = null)
        {
            var query = _dbSet.Where(sc =>
                sc.Name.ToLower() == name.ToLower() &&
                sc.CredentialId == credentialId);

            if (excludeId.HasValue)
            {
                query = query.Where(sc => sc.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> IsSpreadsheetIdExistsAsync(string spreadsheetId, int? excludeId = null)
        {
            var query = _dbSet.Where(sc => sc.SpreadsheetId == spreadsheetId);

            if (excludeId.HasValue)
            {
                query = query.Where(sc => sc.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> GetRaceCountAsync(int sheetConfigId)
        {
            return await _context.Races
                .CountAsync(r => r.SheetConfigId == sheetConfigId);
        }

        public async Task UpdateLastSyncRowAsync(int configId, int rowIndex)
        {
            var config = await _dbSet.FindAsync(configId);
            if (config != null)
            {
                config.LastSyncRowIndex = rowIndex;
                config.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<GoogleSheetConfig>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetConfigsWithCredentialsAsync();
            }

            var term = searchTerm.ToLower();

            return await _dbSet
                .Include(sc => sc.Credential)
                .Include(sc => sc.Races)
                .Where(sc =>
                    sc.Name.ToLower().Contains(term) ||
                    sc.Description != null && sc.Description.ToLower().Contains(term) ||
                    sc.SpreadsheetId.Contains(term) ||
                    sc.SheetName != null && sc.SheetName.ToLower().Contains(term) ||
                    sc.Credential.Name.ToLower().Contains(term))
                .OrderBy(sc => sc.Name)
                .ToListAsync();
        }
    }
}
