using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.DTOs
{
    // Main Dashboard Overview DTO
    public class DashboardOverviewDto
    {
        // Overall Statistics
        public int TotalRaces { get; set; }
        public int ActiveRaces { get; set; }
        public int TotalRegistrations { get; set; }
        public int TodayRegistrations { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }

        // Email Statistics
        public EmailDashboardStats EmailStats { get; set; } = new();

        // Recent Activities
        public List<RecentActivityDto> RecentActivities { get; set; } = new();

        // Chart Data
        public List<RegistrationTrendDto> RegistrationTrends { get; set; } = new();
        public List<RevenueTrendDto> RevenueTrends { get; set; } = new();
        public List<RaceStatusDistributionDto> RaceStatusDistribution { get; set; } = new();

        // Quick Stats
        public int PendingPayments { get; set; }
        public int BibsToGenerate { get; set; }
        public int UpcomingRaces { get; set; }
        public double AverageRegistrationsPerRace { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    // Email Dashboard Statistics
    public class EmailDashboardStats
    {
        public int TotalEmailsSent { get; set; }
        public int PendingEmails { get; set; }
        public int FailedEmails { get; set; }
        public int EmailsSentToday { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan? AverageDeliveryTime { get; set; }

        public List<EmailTypeStats> EmailTypeBreakdown { get; set; } = new();
    }

    public class EmailTypeStats
    {
        public string EmailType { get; set; } = string.Empty;
        public int Sent { get; set; }
        public int Failed { get; set; }
        public int Pending { get; set; }
    }

    // Recent Activity DTO
    public class RecentActivityDto
    {
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = "blue";
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    // Chart Data DTOs
    public class RegistrationTrendDto
    {
        public DateTime Date { get; set; }
        public int RegistrationCount { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class RevenueTrendDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class RaceStatusDistributionDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}
