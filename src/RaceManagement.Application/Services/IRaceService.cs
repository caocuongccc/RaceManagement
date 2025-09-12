using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaceManagement.Shared.DTOs;

namespace RaceManagement.Application.Services
{
    public interface IRaceService
    {
        Task<RaceDto> CreateRaceAsync(CreateRaceDto dto);
        Task<RaceDto?> GetRaceAsync(int id);
        Task<IEnumerable<RaceDto>> GetActiveRacesAsync();
        Task<RaceStatisticsDto> GetRaceStatisticsAsync(int raceId);
    }
}
