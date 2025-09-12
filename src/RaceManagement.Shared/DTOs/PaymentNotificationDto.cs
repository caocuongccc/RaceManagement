using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.DTOs
{
    public class PaymentNotificationDto
    {
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? BankCode { get; set; }
        public DateTime? TransactionTime { get; set; }
        public string TransactionReference { get; set; } = string.Empty;
    }
}
