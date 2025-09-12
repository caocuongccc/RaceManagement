using RaceManagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRaceRepository Races { get; }
        IRegistrationRepository Registrations { get; }
        IRepository<RaceDistance> RaceDistances { get; }
        IRepository<Payment> Payments { get; }
        IRepository<EmailLog> EmailLogs { get; }
        IRaceShirtTypeRepository RaceShirtTypes { get; }             // NEW

        // NEW - Credential Management
        ICredentialRepository Credentials { get; }               // NEW
        ISheetConfigRepository SheetConfigs { get; }
        // Add to IUnitOfWork interface:
        IEmailQueueRepository EmailQueues { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
