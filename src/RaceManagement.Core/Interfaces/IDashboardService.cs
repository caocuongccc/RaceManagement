using RaceManagement.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Core.Interfaces
{
    public interface IDashboardService
    {
        // Overview Dashboard
        Task<DashboardOverviewDto> GetDashboardOverviewAsync();
        Task<DashboardOverviewDto> GetDashboardOverviewAsync(DateTime fromDate, DateTime toDate);

        // Race Analytics
        Task<RaceAnalyticsDto> GetRaceAnalyticsAsync(int raceId);
        Task<List<RaceAnalyticsDto>> GetTopPerformingRacesAsync(int count = 10);

        // Trends and Charts
        Task<List<RegistrationTrendDto>> GetRegistrationTrendsAsync(int days = 30);
        Task<List<RevenueTrendDto>> GetRevenueTrendsAsync(int days = 30);
        Task<List<RaceStatusDistributionDto>> GetRaceStatusDistributionAsync();

        // Recent Activities
        Task<List<RecentActivityDto>> GetRecentActivitiesAsync(int count = 20);
        Task AddActivityAsync(string activityType, string description, string userName, Dictionary<string, object>? metadata = null);

        // System Health
        Task<SystemHealthDto> GetSystemHealthAsync();
        Task<bool> RunHealthChecksAsync();

        // Performance Metrics
        Task<Dictionary<string, object>> GetPerformanceMetricsAsync();
        Task<List<object>> GetCustomReportAsync(string reportType, Dictionary<string, object> parameters);
    }

    public interface IDashboardRepository
    {
        Task<Dictionary<DateTime, int>> GetRegistrationTrendDataAsync(DateTime fromDate, DateTime toDate);
        Task<Dictionary<DateTime, decimal>> GetRevenueTrendDataAsync(DateTime fromDate, DateTime toDate);
        Task<List<RacePerformanceData>> GetRacePerformanceDataAsync(int topCount = 10);
        Task<Dictionary<string, int>> GetRaceStatusCountsAsync();
        Task<List<ActivityLogEntry>> GetRecentActivitiesAsync(int count = 20);
        Task<SystemMetrics> GetSystemMetricsAsync();
    }
}
