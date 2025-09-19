using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.Enums
{
    public enum EmailType
    {
        RegistrationConfirm = 1,
        BibNumber = 2,
        PaymentReminder = 3,
        RaceDayInfo = 4,
        PaymentReceived = 5,
        WelcomeEmail = 6,
        CustomNotification = 7
    }
}
