using RaceManagement.Core.Entities;
using RaceManagement.Abstractions.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Core.Interfaces
{
    public interface IEmailQueueRepository : IRepository<EmailQueue>
    {
        Task<IEnumerable<EmailQueue>> GetPendingEmailsAsync(int batchSize = 10);
        Task<IEnumerable<EmailQueue>> GetScheduledEmailsAsync(DateTime currentTime);
        Task<IEnumerable<EmailQueue>> GetFailedEmailsForRetryAsync(int batchSize = 10);
        Task<IEnumerable<EmailQueue>> GetEmailsByRegistrationAsync(int registrationId);
        Task<IEnumerable<EmailQueue>> GetEmailsByTypeAsync(EmailType emailType);
        Task<EmailQueue?> GetByRegistrationAndTypeAsync(int registrationId, EmailType emailType);
        Task<int> GetPendingCountAsync();
        Task<int> GetProcessingCountAsync();
        Task<Dictionary<EmailStatus, int>> GetStatusCountsAsync();
        Task MarkAsProcessingAsync(IEnumerable<int> emailIds);
        Task<IEnumerable<EmailQueue>> GetQueueStatusAsync(int take = 50);
    }
}
