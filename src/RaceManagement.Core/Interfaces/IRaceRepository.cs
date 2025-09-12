using RaceManagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Core.Interfaces
{
    public interface IRaceRepository : IRepository<Race>
    {
        Task<IEnumerable<Race>> GetActiveRacesAsync();
        Task<Race?> GetRaceWithDistancesAsync(int id);
        Task<Race?> GetRaceWithFullDetailsAsync(int id);

         // This was missing
        Task<IEnumerable<Race>> GetRacesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Race?> GetRaceWithRegistrationsAsync(int raceId);
    }
}
