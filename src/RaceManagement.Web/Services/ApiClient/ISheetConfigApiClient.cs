using RaceManagement.Shared.DTOs;

namespace RaceManagement.Web.Services.ApiClient
{
    public interface ISheetConfigApiClient
    {
        Task<List<SheetConfigDto>> GetAllAsync();
        Task<SheetConfigDto?> GetByIdAsync(int id);
        Task<bool> CreateAsync(CreateSheetConfigDto dto);
        Task<bool> UpdateAsync(int id, UpdateSheetConfigDto dto);
    }
}
