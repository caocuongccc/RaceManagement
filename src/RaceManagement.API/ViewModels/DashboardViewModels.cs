using RaceManagement.Shared.DTOs;

namespace RaceManagement.API.ViewModels
{
    // Main Dashboard ViewModel
    public class DashboardViewModel
    {
        public DashboardOverviewDto Overview { get; set; } = new();
        public List<RaceAnalyticsDto> TopRaces { get; set; } = new();
        public SystemHealthDto SystemHealth { get; set; } = new();
        public string PageTitle { get; set; } = "Dashboard";
        public DateTime LastRefresh { get; set; } = DateTime.Now;
        public bool HasErrors { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;

        // UI State properties
        public bool ShowCharts { get; set; } = true;
        public bool ShowEmailStats { get; set; } = true;
        public bool ShowRecentActivities { get; set; } = true;
        public int ChartDays { get; set; } = 30;

        // Quick stats for easy binding
        public string FormattedTotalRevenue => Overview.TotalRevenue.ToString("C0");
        public string FormattedMonthlyRevenue => Overview.MonthlyRevenue.ToString("C0");
        public double EmailSuccessRate => Overview.EmailStats.SuccessRate;
        public bool HasPendingIssues => Overview.PendingPayments > 0 || Overview.BibsToGenerate > 0;
    }

    // Race Analytics ViewModel
    public class RaceAnalyticsViewModel
    {
        public RaceAnalyticsDto Analytics { get; set; } = new();
        public List<RegistrationTrendDto> RegistrationTrends { get; set; } = new();
        public List<RevenueTrendDto> RevenueTrends { get; set; } = new();
        public string PageTitle { get; set; } = "Race Analytics";
        public bool HasData => Analytics.RaceId > 0;

        // Formatted properties
        public string FormattedTotalRevenue => Analytics.TotalRevenue.ToString("C0");
        public string FormattedPotentialRevenue => Analytics.PotentialRevenue.ToString("C0");
        public string FormattedAverageValue => Analytics.AverageRegistrationValue.ToString("C2");
        public string CompletionRateFormatted => $"{Analytics.CompletionRate:F1}%";
        //public string TrendDirectionIcon => Analytics.TrendDirection switch
        //{
        //    "up" => "fas fa-arrow-up text-success",
        //    "down" => "fas fa-arrow-down text-danger",
        //    _ => "fas fa-minus text-muted"
        //};
        //public string TrendDirectionText => Analytics.TrendDirection switch
        //{
        //    "up" => "Increasing",
        //    "down" => "Decreasing",
        //    _ => "Stable"
        //};
        // FIX: Add TrendDirectionIcon property
        public string TrendDirectionIcon => Analytics.TrendDirection switch
        {
            "up" => "fas fa-arrow-up text-success",
            "down" => "fas fa-arrow-down text-danger",
            _ => "fas fa-minus text-muted"
        };

        public string TrendDirectionText => Analytics.TrendDirection switch
        {
            "up" => "Increasing",
            "down" => "Decreasing",
            _ => "Stable"
        };
    }

    // System Health ViewModel
    public class SystemHealthViewModel
    {
        public SystemHealthDto Health { get; set; } = new();
        public string PageTitle { get; set; } = "System Health";
        public DateTime LastCheck { get; set; } = DateTime.Now;

        // Health status indicators
        public string OverallStatusColor => Health.Status switch
        {
            "Healthy" => "success",
            "Warning" => "warning",
            "Critical" => "danger",
            _ => "secondary"
        };

        public string OverallStatusIcon => Health.Status switch
        {
            "Healthy" => "fas fa-check-circle",
            "Warning" => "fas fa-exclamation-triangle",
            "Critical" => "fas fa-times-circle",
            _ => "fas fa-question-circle"
        };

        public string EmailQueueStatusColor => Health.EmailQueueHealth.HealthStatus switch
        {
            "Good" => "success",
            "Warning" => "warning",
            "Critical" => "danger",
            _ => "secondary"
        };

        public bool HasEmailIssues => Health.EmailQueueHealth.FailedCount > 10 ||
                                     Health.EmailQueueHealth.PendingCount > 100;
    }

    // Reports ViewModel
    public class ReportsViewModel
    {
        public List<RaceAnalyticsDto> TopPerformingRaces { get; set; } = new();
        public List<RegistrationTrendDto> OverallTrends { get; set; } = new();
        public List<RevenueTrendDto> RevenueTrends { get; set; } = new();
        public Dictionary<string, object> CustomMetrics { get; set; } = new();
        public string PageTitle { get; set; } = "Reports";
        public DateTime ReportDate { get; set; } = DateTime.Now;

        // Report filters
        public DateTime FromDate { get; set; } = DateTime.Now.AddMonths(-3);
        public DateTime ToDate { get; set; } = DateTime.Now;
        public string SelectedPeriod { get; set; } = "3months";

        // Summary calculations
        public decimal TotalRevenue => TopPerformingRaces.Sum(r => r.TotalRevenue);
        public int TotalRegistrations => TopPerformingRaces.Sum(r => r.TotalRegistrations);
        public double AverageCompletionRate => TopPerformingRaces.Any() ?
            TopPerformingRaces.Average(r => (double)r.CompletionRate) : 0;
    }

    // Chart Data ViewModels
    public class ChartDataViewModel
    {
        public string ChartType { get; set; } = "line";
        public List<string> Labels { get; set; } = new();
        public List<ChartDatasetViewModel> Datasets { get; set; } = new();
        public Dictionary<string, object> Options { get; set; } = new();
    }

    public class ChartDatasetViewModel
    {
        public string Label { get; set; } = string.Empty;
        public List<decimal> Data { get; set; } = new();
        public string BorderColor { get; set; } = "#007bff";
        public string BackgroundColor { get; set; } = "rgba(0, 123, 255, 0.1)";
        public double Tension { get; set; } = 0.1;
    }

    // Widget ViewModels for partial views
    public class StatsWidgetViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string SubText { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string ColorClass { get; set; } = "primary";
        public string Url { get; set; } = "#";
    }

    public class ActivityFeedViewModel
    {
        public List<RecentActivityDto> Activities { get; set; } = new();
        public int MaxItems { get; set; } = 10;
        public bool ShowLoadMore { get; set; } = true;
        public string ContainerId { get; set; } = "activityFeed";
    }

    // Filter ViewModels
    public class DateRangeFilterViewModel
    {
        public DateTime FromDate { get; set; } = DateTime.Now.AddDays(-30);
        public DateTime ToDate { get; set; } = DateTime.Now;
        public List<PredefinedPeriod> PredefinedPeriods { get; set; } = new()
        {
            new PredefinedPeriod { Label = "Last 7 days", Days = 7 },
            new PredefinedPeriod { Label = "Last 30 days", Days = 30 },
            new PredefinedPeriod { Label = "Last 3 months", Days = 90 },
            new PredefinedPeriod { Label = "Last 6 months", Days = 180 },
            new PredefinedPeriod { Label = "Last year", Days = 365 }
        };
    }

    public class PredefinedPeriod
    {
        public string Label { get; set; } = string.Empty;
        public int Days { get; set; }
    }

    // Form ViewModels
    public class HealthCheckRequestViewModel
    {
        public bool RunDatabaseCheck { get; set; } = true;
        public bool RunEmailQueueCheck { get; set; } = true;
        public bool RunPerformanceCheck { get; set; } = true;
        public bool SendNotifications { get; set; } = false;
    }

    public class CustomReportRequestViewModel
    {
        public string ReportType { get; set; } = string.Empty;
        public DateTime FromDate { get; set; } = DateTime.Now.AddMonths(-1);
        public DateTime ToDate { get; set; } = DateTime.Now;
        public List<int> SelectedRaceIds { get; set; } = new();
        public string Format { get; set; } = "html"; // html, pdf, excel
        public Dictionary<string, string> Parameters { get; set; } = new();
    }
}
