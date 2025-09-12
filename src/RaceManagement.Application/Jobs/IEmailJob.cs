using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Application.Jobs
{
    public interface IEmailJob
    {
        Task ProcessPendingEmailsAsync();
        Task ProcessScheduledEmailsAsync();
        Task RetryFailedEmailsAsync();
        Task SendRegistrationConfirmationEmailAsync(int registrationId);
        Task SendBibNotificationEmailAsync(int registrationId);
        Task SendPaymentReminderEmailsAsync(int raceId);
        Task SendRaceDayInfoEmailsAsync(int raceId);
        Task CleanupOldEmailLogsAsync(int daysToKeep = 30);
    }
}
