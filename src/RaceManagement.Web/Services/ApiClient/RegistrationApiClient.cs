using RaceManagement.Shared.DTOs;

namespace RaceManagement.Web.Services.ApiClient
{
    public class RegistrationApiClient : IRegistrationApiClient
    {
        private readonly HttpClient _http;

        public RegistrationApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("RaceApi");
        }

        public async Task<List<RegistrationDto>> GetByRaceAsync(int raceId)
        {
            var res = await _http.GetAsync($"registrations/race/{raceId}");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<RegistrationDto>>() ?? new List<RegistrationDto>();
        }

        public async Task<bool> MarkPaidAsync(int registrationId)
        {
            var res = await _http.PostAsync($"registrations/{registrationId}/mark-paid", null);
            return res.IsSuccessStatusCode;
        }
    }
}
