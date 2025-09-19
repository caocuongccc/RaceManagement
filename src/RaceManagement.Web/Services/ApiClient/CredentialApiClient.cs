using RaceManagement.Shared.DTOs;

namespace RaceManagement.Web.Services.ApiClient
{
    public class CredentialApiClient : ICredentialApiClient
    {
        private readonly HttpClient _http;

        public CredentialApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("RaceApi");
        }

        public async Task<List<CredentialDto>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<CredentialDto>>("credentials") ?? new List<CredentialDto>();
        }

        public async Task<CredentialDto?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<CredentialDto>($"credentials/{id}");
        }

        //public async Task<bool> CreateAsync(CreateCredentialDto dto)
        //{
        //    var res = await _http.PostAsJsonAsync("credentials", dto);
        //    return res.IsSuccessStatusCode;
        //}

        public async Task<bool> CreateAsync(CreateCredentialDto dto)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(dto.Name), nameof(dto.Name));
            if (!string.IsNullOrEmpty(dto.Description))
                content.Add(new StringContent(dto.Description), nameof(dto.Description));
            if (!string.IsNullOrEmpty(dto.CreatedBy))
                content.Add(new StringContent(dto.CreatedBy), nameof(dto.CreatedBy));

            if (dto.CredentialFile != null)
            {
                var fileContent = new StreamContent(dto.CredentialFile.OpenReadStream());
                content.Add(fileContent, nameof(dto.CredentialFile), dto.CredentialFile.FileName);
            }

            var res = await _http.PostAsync("credentials", content);
            return res.IsSuccessStatusCode;
        }
    }
}
