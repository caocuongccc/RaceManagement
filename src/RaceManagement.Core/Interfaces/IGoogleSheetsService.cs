using RaceManagement.Shared.DTOs;

namespace RaceManagement.Core.Interfaces
{
    public interface IGoogleSheetsService
    {
        Task<IEnumerable<SheetRegistrationDto>> ReadNewRegistrationsAsync(
            string sheetId, int fromRowIndex, string? credentialPath = null);
        Task<string> CreatePaymentTrackingSheetAsync(
            string sourceSheetId, string raceName, string? credentialPath = null);
        Task MarkPaymentStatusAsync(
            string sheetId, int rowIndex, bool isPaid, string? credentialPath = null);
        Task<bool> TestConnectionAsync(string sheetId, string? credentialPath = null);
        Task<int> GetTotalRowsAsync(string sheetId, string range = "A:A", string? credentialPath = null);

        // NEW: Additional methods
        Task<SheetMetadataDto> GetSheetMetadataAsync(string sheetId, string? credentialPath = null);
        Task<IEnumerable<string>> GetSheetNamesAsync(string spreadsheetId, string? credentialPath = null);
        void ClearCredentialCache(); // For switching between credentials
    }
}
