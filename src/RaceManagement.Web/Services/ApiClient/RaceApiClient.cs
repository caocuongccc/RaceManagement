using RaceManagement.Shared.DTOs;

namespace RaceManagement.Web.Services.ApiClient
{
    public class RaceApiClient : IRaceApiClient
    {
        private readonly HttpClient _http;
        public RaceApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("RaceApi");
        }

        public async Task<List<RaceDto>> GetAllAsync()
        {
            var res = await _http.GetAsync("races");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<RaceDto>>() ?? new List<RaceDto>();
        }

        public async Task<RaceDto?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<RaceDto>($"races/{id}");
        }

        public async Task<bool> CreateAsync(CreateRaceDto dto)
        {
            var res = await _http.PostAsJsonAsync("races", dto);
            return res.IsSuccessStatusCode;
        }
    }
}
