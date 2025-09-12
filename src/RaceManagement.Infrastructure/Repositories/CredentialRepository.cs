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
    public class CredentialRepository : Repository<GoogleCredential>, ICredentialRepository
    {
        public CredentialRepository(RaceManagementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<GoogleCredential>> GetActiveCredentialsAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .Include(c => c.SheetConfigs.Where(sc => sc.IsActive))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<GoogleCredential>> GetCredentialsWithSheetConfigsAsync()
        {
            return await _dbSet
                .Include(c => c.SheetConfigs)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<GoogleCredential?> GetByNameAsync(string name)
        {
            return await _dbSet
                .Include(c => c.SheetConfigs)
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<GoogleCredential?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .Include(c => c.SheetConfigs)
                .FirstOrDefaultAsync(c => c.ServiceAccountEmail.ToLower() == email.ToLower());
        }

        public async Task<GoogleCredential?> GetWithSheetConfigsAsync(int id)
        {
            return await _dbSet
                .Include(c => c.SheetConfigs.Where(sc => sc.IsActive))
                    .ThenInclude(sc => sc.Races)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(c => c.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> IsEmailExistsAsync(string email, int? excludeId = null)
        {
            var query = _dbSet.Where(c => c.ServiceAccountEmail.ToLower() == email.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> GetSheetConfigCountAsync(int credentialId)
        {
            return await _context.GoogleSheetConfigs
                .CountAsync(sc => sc.CredentialId == credentialId);
        }

        public async Task<IEnumerable<GoogleCredential>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetCredentialsWithSheetConfigsAsync();
            }

            var term = searchTerm.ToLower();

            return await _dbSet
                .Include(c => c.SheetConfigs)
                .Where(c =>
                    c.Name.ToLower().Contains(term) ||
                    c.Description != null && c.Description.ToLower().Contains(term) ||
                    c.ServiceAccountEmail.ToLower().Contains(term) ||
                    c.CreatedBy != null && c.CreatedBy.ToLower().Contains(term))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}
