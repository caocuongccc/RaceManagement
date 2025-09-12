using Microsoft.EntityFrameworkCore.Storage;
using RaceManagement.Core.Entities;
using RaceManagement.Core.Interfaces;
using RaceManagement.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly RaceManagementDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(RaceManagementDbContext context)
        {
            _context = context;
            Races = new RaceRepository(_context);
            Registrations = new RegistrationRepository(_context);
            RaceDistances = new Repository<RaceDistance>(_context);
            Payments = new Repository<Payment>(_context);
            EmailLogs = new Repository<EmailLog>(_context);
            RaceShirtTypes = new RaceShirtTypeRepository(_context);  // NEW
            Credentials = new CredentialRepository(_context);      // NEW
            SheetConfigs = new SheetConfigRepository(_context);   // NEW
            EmailQueues = new EmailQueueRepository(_context);

        }

        public IRaceRepository Races { get; }
        public IRegistrationRepository Registrations { get; }
        public IRepository<RaceDistance> RaceDistances { get; }
        public IRepository<Payment> Payments { get; }
        public IRepository<EmailLog> EmailLogs { get; }
        public IRaceShirtTypeRepository RaceShirtTypes { get; }      // NEW
        // NEW - Credential Management properties
        public ICredentialRepository Credentials { get; }         // NEW
        public ISheetConfigRepository SheetConfigs { get; }       // NEW
        // Add to IUnitOfWork interface:
        public IEmailQueueRepository EmailQueues { get; }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
