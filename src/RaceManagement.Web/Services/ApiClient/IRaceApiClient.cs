using RaceManagement.Shared.DTOs;

namespace RaceManagement.Web.Services.ApiClient
{
    public interface IRaceApiClient
    {
        Task<List<RaceDto>> GetAllAsync();
        Task<RaceDto?> GetByIdAsync(int id);
        Task<bool> CreateAsync(CreateRaceDto dto);
    }
}
