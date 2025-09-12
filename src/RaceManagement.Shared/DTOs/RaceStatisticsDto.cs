using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.DTOs
{
    public class RaceStatisticsDto
    {
        public int RaceId { get; set; }
        public string RaceName { get; set; } = string.Empty;
        public int TotalRegistrations { get; set; }
        public int PendingPayments { get; set; }
        public int PaidRegistrations { get; set; }
        public int BibsGenerated { get; set; }
        public int BibsSent { get; set; }
        public decimal TotalRevenue { get; set; }
        public Dictionary<string, int> RegistrationsByDistance { get; set; } = new();
        public Dictionary<string, int> RegistrationsByShirtCategory { get; set; } = new(); // NEW
        public Dictionary<string, int> RegistrationsByShirtSize { get; set; } = new();     // NEW
        public Dictionary<string, int> RegistrationsByGender { get; set; } = new();        // NEW
        public Dictionary<string, int> RegistrationsByAgeGroup { get; set; } = new();      // NEW

        // Computed properties
        public decimal PaidPercentage => TotalRegistrations > 0
            ? (decimal)PaidRegistrations / TotalRegistrations * 100
            : 0;

        public decimal BibSentPercentage => PaidRegistrations > 0
            ? (decimal)BibsSent / PaidRegistrations * 100
            : 0;

        public string RevenueDisplay => $"{TotalRevenue:N0} VNĐ";
    }
}
