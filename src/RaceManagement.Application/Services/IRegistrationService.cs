using RaceManagement.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Application.Services
{
    public interface IRegistrationService
    {
        Task<SyncResultDto> SyncRegistrationsFromSheetAsync(int raceId);
        Task<RegistrationDto?> GetRegistrationAsync(int id);
        Task<IEnumerable<RegistrationDto>> GetRegistrationsByRaceAsync(int raceId);
        Task ProcessPaymentAsync(int registrationId, PaymentNotificationDto notification);
        Task<string> GenerateBibNumberAsync(int registrationId);
        Task<string> GenerateTransactionReference();
    }
}
