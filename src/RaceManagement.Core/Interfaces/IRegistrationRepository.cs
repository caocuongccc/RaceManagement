using RaceManagement.Shared.Enums;
using RaceManagement.Core.Entities;

namespace RaceManagement.Core.Interfaces
{
    public interface IRegistrationRepository : IRepository<Registration>
    {
        Task<Registration?> GetByIdWithIncludesAsync(int id);
        Task<Registration?> GetByTransactionReferenceAsync(string transactionReference);
        Task<int> GetLastSheetRowIndexAsync(int raceId);
        //Task<IEnumerable<Registration>> GetByRaceIdAsync(int raceId);
        Task<IEnumerable<Registration>> GetPendingPaymentsAsync(int raceId);
        //Task<IEnumerable<Registration>> GetPaidRegistrationsAsync(int raceId);
        //Task<IEnumerable<Registration>> GetPendingPaymentRegistrationsAsync(int raceId);
        //Task<Registration> GetRegistrationWithDetailsAsync(int raceId);
        // Core methods
        Task<Registration?> GetRegistrationWithDetailsAsync(int registrationId);
        Task<IEnumerable<Registration>> GetPendingPaymentRegistrationsAsync(int raceId);
        Task<IEnumerable<Registration>> GetPaidRegistrationsAsync(int raceId);
        Task<IEnumerable<Registration>> GetByRaceIdAsync(int raceId);
        Task<List<Registration>> GetAllWithDetailsAsync();

        // Additional helper methods
        Task<Registration?> GetRegistrationByEmailAndRaceAsync(string email, int raceId);
        Task<IEnumerable<Registration>> GetRegistrationsWithBibNumbersAsync(int raceId);
        Task<IEnumerable<Registration>> GetRegistrationsNeedingBibAssignmentAsync(int raceId);
        Task<bool> IsEmailAlreadyRegisteredAsync(string email, int raceId);
        Task<int> GetRegistrationCountByStatusAsync(int raceId, PaymentStatus status);
        Task<Dictionary<PaymentStatus, int>> GetRegistrationStatusSummaryAsync(int raceId);
    }
}
