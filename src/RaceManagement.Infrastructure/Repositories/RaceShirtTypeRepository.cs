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
    public class RaceShirtTypeRepository : Repository<RaceShirtType>, IRaceShirtTypeRepository
    {
        public RaceShirtTypeRepository(RaceManagementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<RaceShirtType>> GetByRaceIdAsync(int raceId)
        {
            return await _dbSet
                .Where(st => st.RaceId == raceId)
                .OrderBy(st => st.ShirtCategory)
                .ThenBy(st => st.ShirtType)
                .ToListAsync();
        }

        public async Task<IEnumerable<RaceShirtType>> GetActiveByRaceIdAsync(int raceId)
        {
            return await _dbSet
                .Where(st => st.RaceId == raceId && st.IsActive)
                .OrderBy(st => st.ShirtCategory)
                .ThenBy(st => st.ShirtType)
                .ToListAsync();
        }

        public async Task<RaceShirtType?> FindByRaceCategoryTypeAsync(int raceId, string category, string type)
        {
            return await _dbSet
                .FirstOrDefaultAsync(st => st.RaceId == raceId &&
                                          st.ShirtCategory == category &&
                                          st.ShirtType == type &&
                                          st.IsActive);
        }

        public async Task<Dictionary<string, List<string>>> GetAvailableSizesByRaceAsync(int raceId)
        {
            var shirtTypes = await GetActiveByRaceIdAsync(raceId);

            return shirtTypes
                .GroupBy(st => st.ShirtCategory)
                .ToDictionary(
                    g => g.Key,
                    g => g.SelectMany(st => st.GetSizesList()).Distinct().ToList()
                );
        }
    }
}
