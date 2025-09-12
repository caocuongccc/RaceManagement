using RaceManagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Core.Interfaces
{
    public interface IRaceShirtTypeRepository : IRepository<RaceShirtType>
    {
        Task<IEnumerable<RaceShirtType>> GetByRaceIdAsync(int raceId);
        Task<IEnumerable<RaceShirtType>> GetActiveByRaceIdAsync(int raceId);
        Task<RaceShirtType?> FindByRaceCategoryTypeAsync(int raceId, string category, string type);
        Task<Dictionary<string, List<string>>> GetAvailableSizesByRaceAsync(int raceId);
    }
}
