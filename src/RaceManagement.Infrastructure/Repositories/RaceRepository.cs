using Microsoft.EntityFrameworkCore;
using RaceManagement.Core.Entities;
using RaceManagement.Abstractions.Enums;
using RaceManagement.Core.Interfaces;
using RaceManagement.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Infrastructure.Repositories
{
    public class RaceRepository : Repository<Race>, IRaceRepository
    {
        public RaceRepository(RaceManagementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Race>> GetActiveRacesAsync()
        {
            return await _dbSet
            .Where(r => r.Status == RaceStatus.Active)
            .Include(r => r.Distances)
            .Include(r => r.ShirtTypes.Where(st => st.IsActive))  // NEW: Include shirt types
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        }

        public async Task<Race?> GetRaceWithDistancesAsync(int id)
        {
            return await _dbSet
            .Include(r => r.Distances)
            .Include(r => r.ShirtTypes.Where(st => st.IsActive))  // NEW: Include shirt types
            .FirstOrDefaultAsync(r => r.Id == id);
        }

        // NEW: Get race with full details including registrations
        public async Task<Race?> GetRaceWithFullDetailsAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Distances)
                .Include(r => r.ShirtTypes)
                .Include(r => r.Registrations)
                    .ThenInclude(reg => reg.Distance)
                .Include(r => r.Registrations)
                    .ThenInclude(reg => reg.Payments)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // NEW: Get races by date range
        public async Task<IEnumerable<Race>> GetRacesByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Where(r => r.RaceDate >= fromDate && r.RaceDate <= toDate)
                .Include(r => r.Distances)
                .Include(r => r.ShirtTypes.Where(st => st.IsActive))
                .OrderBy(r => r.RaceDate)
                .ToListAsync();
        }

        // NEW: Get upcoming races
        public async Task<IEnumerable<Race>> GetUpcomingRacesAsync(int limit = 10)
        {
            return await _dbSet
                .Where(r => r.RaceDate > DateTime.Now && r.Status == RaceStatus.Active)
                .Include(r => r.Distances)
                .Include(r => r.ShirtTypes.Where(st => st.IsActive))
                .OrderBy(r => r.RaceDate)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<Race?> GetRaceWithRegistrationsAsync(int raceId)
        {
            return await _context.Races
                .Include(r => r.Registrations)
                    .ThenInclude(reg => reg.Distance)
                .Include(r => r.Registrations)
                    .ThenInclude(reg => reg.ShirtType)
                .Include(r => r.Distances)
                .Include(r => r.ShirtTypes)
                .AsSplitQuery()
                .FirstOrDefaultAsync(r => r.Id == raceId);
        }
    }
}
