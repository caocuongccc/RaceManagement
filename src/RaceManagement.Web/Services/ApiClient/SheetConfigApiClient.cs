using RaceManagement.Shared.DTOs;

namespace RaceManagement.Web.Services.ApiClient
{
    public class SheetConfigApiClient : ISheetConfigApiClient
    {
        private readonly HttpClient _http;

        public SheetConfigApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("RaceApi");
        }

        public async Task<List<SheetConfigDto>> GetAllAsync()
        {
            var res = await _http.GetAsync("sheetconfigs");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<SheetConfigDto>>() ?? new List<SheetConfigDto>();
        }

        public async Task<SheetConfigDto?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<SheetConfigDto>($"sheetconfigs/{id}");
        }

        public async Task<bool> CreateAsync(CreateSheetConfigDto dto)
        {
            var res = await _http.PostAsJsonAsync("sheetconfigs", dto);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(int id, UpdateSheetConfigDto dto)
        {
            var res = await _http.PutAsJsonAsync($"sheetconfigs/{id}", dto);
            return res.IsSuccessStatusCode;
        }
    }
}
