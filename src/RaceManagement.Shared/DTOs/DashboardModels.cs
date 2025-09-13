using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.DTOs
{
    // Model để trả về performance data của races
    public class RacePerformanceData
    {
        public int RaceId { get; set; }
        public string RaceName { get; set; } = string.Empty;
        public DateTime RaceDate { get; set; }
        public int TotalRegistrations { get; set; }
        public decimal TotalRevenue { get; set; }
        public double CompletionRate { get; set; }
    }

    // Model để lưu activity log
    public class ActivityLogEntry
    {
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    // Model cho system metrics
    public class SystemMetrics
    {
        public int TotalRaces { get; set; }
        public int TotalRegistrations { get; set; }
        public int TotalPendingEmails { get; set; }
        public int TotalFailedEmails { get; set; }
        public long DatabaseSize { get; set; }
        public DateTime LastCalculated { get; set; }
    }
}
