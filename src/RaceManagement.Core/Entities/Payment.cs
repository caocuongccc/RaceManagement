using RaceManagement.Abstractions.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Core.Entities
{
    public class Payment : BaseEntity
    {
        public int RegistrationId { get; set; }
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string? BankCode { get; set; }
        public DateTime? TransactionTime { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending; 
        public string? PaymentMethod { get; set; }

        // Navigation properties
        public virtual Registration Registration { get; set; } = null!;
    }
}
