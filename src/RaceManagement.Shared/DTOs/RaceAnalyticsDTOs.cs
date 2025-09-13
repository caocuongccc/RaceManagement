using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.DTOs
{
    // Race Analytics DTO
    public class RaceAnalyticsDto
    {
        public int RaceId { get; set; }
        public string RaceName { get; set; } = string.Empty;
        public DateTime RaceDate { get; set; }

        // Registration Analytics
        public int TotalRegistrations { get; set; }
        public int PaidRegistrations { get; set; }
        public int PendingRegistrations { get; set; }
        public decimal CompletionRate => TotalRegistrations > 0 ? (decimal)PaidRegistrations / TotalRegistrations * 100 : 0;

        // Revenue Analytics
        public decimal TotalRevenue { get; set; }
        public decimal PotentialRevenue { get; set; }
        public decimal AverageRegistrationValue { get; set; }

        // Distance Analytics
        public List<DistanceAnalyticsDto> DistanceAnalytics { get; set; } = new();

        // Registration Timeline
        public List<RegistrationTimelineDto> RegistrationTimeline { get; set; } = new();

        // Performance Metrics
        public double DailyRegistrationRate { get; set; }
        public int DaysUntilRace { get; set; }
        public string TrendDirection { get; set; } = "stable"; // up, down, stable
    }

    public class DistanceAnalyticsDto
    {
        public string Distance { get; set; } = string.Empty;
        public int Registrations { get; set; }
        public int? MaxCapacity { get; set; }
        public decimal Revenue { get; set; }
        public double FillRate => (double)(MaxCapacity > 0 ? (double)Registrations / MaxCapacity * 100 : 0);
        public bool IsPopular { get; set; }
    }

    public class RegistrationTimelineDto
    {
        public DateTime Date { get; set; }
        public int CumulativeRegistrations { get; set; }
        public int DailyRegistrations { get; set; }
        public decimal CumulativeRevenue { get; set; }
    }

    // System Health DTO
    public class SystemHealthDto
    {
        public string Status { get; set; } = "Healthy"; // Healthy, Warning, Critical
        public List<HealthCheckDto> HealthChecks { get; set; } = new();
        public DateTime LastChecked { get; set; } = DateTime.UtcNow;

        // Performance Metrics
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public long DatabaseSize { get; set; }
        public TimeSpan AverageResponseTime { get; set; }

        // Queue Health
        public EmailQueueHealthDto EmailQueueHealth { get; set; } = new();

        // Recent Errors
        public List<SystemErrorDto> RecentErrors { get; set; } = new();
    }

    public class HealthCheckDto
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public DateTime LastChecked { get; set; }
    }

    public class EmailQueueHealthDto
    {
        public int PendingCount { get; set; }
        public int ProcessingCount { get; set; }
        public int FailedCount { get; set; }
        public TimeSpan? OldestPendingAge { get; set; }
        public string HealthStatus { get; set; } = "Good"; // Good, Warning, Critical
    }

    public class SystemErrorDto
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Severity { get; set; } = string.Empty;
        public int Count { get; set; } = 1;
    }
}
