using Microsoft.EntityFrameworkCore;
using RaceManagement.Core.Entities;
using RaceManagement.Abstractions.Enums;
using RaceManagement.Core.Interfaces;
using RaceManagement.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Infrastructure.Repositories
{
    public class EmailQueueRepository : Repository<EmailQueue>, IEmailQueueRepository
    {
        public EmailQueueRepository(RaceManagementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<EmailQueue>> GetPendingEmailsAsync(int batchSize = 10)
        {
            return await _dbSet
                .Where(e => e.Status == EmailStatus.Pending &&
                           (e.ScheduledAt == null || e.ScheduledAt <= DateTime.Now))
                .Include(e => e.Registration)
                    .ThenInclude(r => r.Race)
                .Include(e => e.Registration)
                    .ThenInclude(r => r.Distance)
                .OrderBy(e => e.Priority)
                .ThenBy(e => e.CreatedAt)
                .Take(batchSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmailQueue>> GetScheduledEmailsAsync(DateTime currentTime)
        {
            return await _dbSet
                .Where(e => e.Status == EmailStatus.Pending &&
                           e.ScheduledAt.HasValue &&
                           e.ScheduledAt <= currentTime)
                .Include(e => e.Registration)
                .OrderBy(e => e.ScheduledAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmailQueue>> GetFailedEmailsForRetryAsync(int batchSize = 10)
        {
            return await _dbSet
                .Where(e => e.Status == EmailStatus.Failed && e.CanRetry)
                .Include(e => e.Registration)
                .OrderBy(e => e.UpdatedAt)
                .Take(batchSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmailQueue>> GetEmailsByRegistrationAsync(int registrationId)
        {
            return await _dbSet
                .Where(e => e.RegistrationId == registrationId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmailQueue>> GetEmailsByTypeAsync(EmailType emailType)
        {
            return await _dbSet
                .Where(e => e.EmailType == emailType)
                .Include(e => e.Registration)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<EmailQueue?> GetByRegistrationAndTypeAsync(int registrationId, EmailType emailType)
        {
            return await _dbSet
                .Where(e => e.RegistrationId == registrationId && e.EmailType == emailType)
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetPendingCountAsync()
        {
            return await _dbSet.CountAsync(e => e.Status == EmailStatus.Pending);
        }

        public async Task<int> GetProcessingCountAsync()
        {
            return await _dbSet.CountAsync(e => e.Status == EmailStatus.Processing);
        }

        public async Task<Dictionary<EmailStatus, int>> GetStatusCountsAsync()
        {
            return await _dbSet
                .GroupBy(e => e.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task MarkAsProcessingAsync(IEnumerable<int> emailIds)
        {
            var emails = await _dbSet
                .Where(e => emailIds.Contains(e.Id))
                .ToListAsync();

            foreach (var email in emails)
            {
                email.MarkAsProcessing();
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<EmailQueue>> GetQueueStatusAsync(int take = 50)
        {
            return await _dbSet
                .Include(e => e.Registration)
                .OrderByDescending(e => e.CreatedAt)
                .Take(take)
                .ToListAsync();
        }
    }
}
