using RaceManagement.Shared.DTOs;

namespace RaceManagement.Web.Services.ApiClient
{
    public interface ICredentialApiClient
    {
        Task<List<CredentialDto>> GetAllAsync();
        Task<CredentialDto?> GetByIdAsync(int id);
        Task<bool> CreateAsync(CreateCredentialDto dto);
    }
}
