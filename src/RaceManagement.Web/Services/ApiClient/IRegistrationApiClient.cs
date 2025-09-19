using RaceManagement.Shared.DTOs;

namespace RaceManagement.Web.Services.ApiClient
{
    public interface IRegistrationApiClient
    {
        Task<List<RegistrationDto>> GetByRaceAsync(int raceId);
        Task<bool> MarkPaidAsync(int registrationId);
    }
}
