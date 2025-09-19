using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RaceManagement.Shared.Enums;
using RaceManagement.Core.Entities;
using RaceManagement.Core.Interfaces;
using RaceManagement.Shared.DTOs;

namespace RaceManagement.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DashboardService> _logger;
        private readonly IMemoryCache _cache;

        public DashboardService(
            IUnitOfWork unitOfWork,
            ILogger<DashboardService> logger,
            IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cache = cache;
        }

        public async Task<DashboardOverviewDto> GetDashboardOverviewAsync()
        {
            var fromDate = DateTime.Now.AddDays(-30);
            var toDate = DateTime.Now;
            return await GetDashboardOverviewAsync(fromDate, toDate);
        }

        public async Task<DashboardOverviewDto> GetDashboardOverviewAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var cacheKey = $"dashboard_overview_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}";

                if (_cache.TryGetValue(cacheKey, out DashboardOverviewDto cachedResult))
                {
                    return cachedResult;
                }

                var overview = new DashboardOverviewDto();

                // Get all races
                var allRaces = await _unitOfWork.Races.GetAllAsync();
                var activeRaces = allRaces.Where(r => r.Status == RaceStatus.Active);

                // Get all registrations
                var allRegistrations = await _unitOfWork.Registrations.GetAllAsync();

                // Basic Statistics
                overview.TotalRaces = allRaces.Count();
                overview.ActiveRaces = activeRaces.Count();
                overview.TotalRegistrations = allRegistrations.Count();
                overview.TodayRegistrations = allRegistrations
                    .Count(r => r.RegistrationTime.Date == DateTime.Today);

                // Revenue Calculations
                var paidRegistrations = allRegistrations.Where(r => r.PaymentStatus == PaymentStatus.Paid);
                overview.TotalRevenue = await CalculateTotalRevenueAsync(paidRegistrations);
                overview.MonthlyRevenue = await CalculateMonthlyRevenueAsync();

                // Email Statistics
                overview.EmailStats = await GetEmailDashboardStatsAsync();

                // Quick Stats
                overview.PendingPayments = allRegistrations
                    .Count(r => r.PaymentStatus == PaymentStatus.Pending);
                overview.BibsToGenerate = allRegistrations
                    .Count(r => r.PaymentStatus == PaymentStatus.Paid && string.IsNullOrEmpty(r.BibNumber));
                overview.UpcomingRaces = activeRaces
                    .Count(r => r.RaceDate > DateTime.Now);
                overview.AverageRegistrationsPerRace = allRaces.Any() ?
                    (double)allRegistrations.Count() / allRaces.Count() : 0;

                // Chart Data
                overview.RegistrationTrends = await GetRegistrationTrendsAsync(30);
                overview.RevenueTrends = await GetRevenueTrendsAsync(30);
                overview.RaceStatusDistribution = await GetRaceStatusDistributionAsync();

                // Recent Activities
                overview.RecentActivities = await GetRecentActivitiesAsync(10);

                // Cache for 5 minutes
                _cache.Set(cacheKey, overview, TimeSpan.FromMinutes(5));

                return overview;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard overview");
                throw;
            }
        }

        public async Task<RaceAnalyticsDto> GetRaceAnalyticsAsync(int raceId)
        {
            try
            {
                var race = await _unitOfWork.Races.GetRaceWithFullDetailsAsync(raceId);
                if (race == null)
                    throw new ArgumentException($"Race {raceId} not found");

                var registrations = race.Registrations;
                var paidRegistrations = registrations.Where(r => r.PaymentStatus == PaymentStatus.Paid);

                var analytics = new RaceAnalyticsDto
                {
                    RaceId = race.Id,
                    RaceName = race.Name,
                    RaceDate = race.RaceDate,
                    TotalRegistrations = registrations.Count(),
                    PaidRegistrations = paidRegistrations.Count(),
                    PendingRegistrations = registrations.Count(r => r.PaymentStatus == PaymentStatus.Pending),
                    DaysUntilRace = Math.Max(0, (int)(race.RaceDate - DateTime.Now).TotalDays)
                };

                // Revenue Analytics
                analytics.TotalRevenue = await CalculateRaceRevenueAsync(raceId);
                analytics.PotentialRevenue = await CalculateRacePotentialRevenueAsync(raceId);
                analytics.AverageRegistrationValue = analytics.TotalRegistrations > 0 ?
                    analytics.TotalRevenue / analytics.TotalRegistrations : 0;

                // Distance Analytics
                analytics.DistanceAnalytics = race.Distances.Select(d => new DistanceAnalyticsDto
                {
                    Distance = d.Distance,
                    Registrations = registrations.Count(r => r.DistanceId == d.Id),
                    MaxCapacity = d.MaxParticipants,
                    Revenue = paidRegistrations.Where(r => r.DistanceId == d.Id).Sum(r => r.Distance.Price),
                    IsPopular = registrations.Count(r => r.DistanceId == d.Id) > analytics.TotalRegistrations * 0.3
                }).ToList();

                // Registration Timeline
                analytics.RegistrationTimeline = GetRegistrationTimeline(registrations);

                // Performance Metrics
                var daysSinceFirstRegistration = registrations.Any() ?
                    (DateTime.Now - registrations.Min(r => r.RegistrationTime)).TotalDays : 0;
                analytics.DailyRegistrationRate = daysSinceFirstRegistration > 0 ?
                    analytics.TotalRegistrations / daysSinceFirstRegistration : 0;

                // Trend Direction
                analytics.TrendDirection = CalculateTrendDirection(analytics.RegistrationTimeline);

                return analytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting race analytics for race {RaceId}", raceId);
                throw;
            }
        }

        // Helper Methods
        private async Task<decimal> CalculateTotalRevenueAsync(IEnumerable<Registration> paidRegistrations)
        {
            return await Task.FromResult(paidRegistrations.Sum(r => r.Distance?.Price ?? 0));
        }

        private async Task<decimal> CalculateMonthlyRevenueAsync()
        {
            var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var monthlyRegistrations = await _unitOfWork.Registrations
                .FindAsync(r => r.PaymentStatus == PaymentStatus.Paid && r.RegistrationTime >= monthStart);

            return monthlyRegistrations.Sum(r => r.Distance?.Price ?? 0);
        }

        private async Task<decimal> CalculateRaceRevenueAsync(int raceId)
        {
            var paidRegistrations = await _unitOfWork.Registrations
                .FindAsync(r => r.RaceId == raceId && r.PaymentStatus == PaymentStatus.Paid);

            return paidRegistrations.Sum(r => r.Distance?.Price ?? 0);
        }

        private async Task<decimal> CalculateRacePotentialRevenueAsync(int raceId)
        {
            var allRegistrations = await _unitOfWork.Registrations
                .FindAsync(r => r.RaceId == raceId);

            return allRegistrations.Sum(r => r.Distance?.Price ?? 0);
        }
        // Add these methods to the existing DashboardService class

        public async Task<List<RegistrationTrendDto>> GetRegistrationTrendsAsync(int days = 30)
        {
            try
            {
                var fromDate = DateTime.Now.AddDays(-days);
                var registrations = await _unitOfWork.Registrations
                    .FindAsync(r => r.RegistrationTime >= fromDate);

                return registrations
                    .GroupBy(r => r.RegistrationTime.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new RegistrationTrendDto
                    {
                        Date = g.Key,
                        RegistrationCount = g.Count(),
                        Label = g.Key.ToString("MMM dd")
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting registration trends");
                throw;
            }
        }

        public async Task<List<RevenueTrendDto>> GetRevenueTrendsAsync(int days = 30)
        {
            try
            {
                var fromDate = DateTime.Now.AddDays(-days);
                var paidRegistrations = await _unitOfWork.Registrations
                    .FindAsync(r => r.PaymentStatus == PaymentStatus.Paid && r.RegistrationTime >= fromDate);

                return paidRegistrations
                    .GroupBy(r => r.RegistrationTime.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new RevenueTrendDto
                    {
                        Date = g.Key,
                        Revenue = g.Sum(r => r.Distance?.Price ?? 0),
                        Label = g.Key.ToString("MMM dd")
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue trends");
                throw;
            }
        }

        public async Task<List<RaceStatusDistributionDto>> GetRaceStatusDistributionAsync()
        {
            try
            {
                var races = await _unitOfWork.Races.GetAllAsync();
                var totalRaces = races.Count();

                return races
                    .GroupBy(r => r.Status)
                    .Select(g => new RaceStatusDistributionDto
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count(),
                        Percentage = totalRaces > 0 ? (decimal)g.Count() / totalRaces * 100 : 0,
                        Color = GetStatusColor(g.Key)
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting race status distribution");
                throw;
            }
        }

        public async Task<List<RecentActivityDto>> GetRecentActivitiesAsync(int count = 20)
        {
            try
            {
                var activities = new List<RecentActivityDto>();

                // Recent registrations
                var recentRegistrations = await _unitOfWork.Registrations
                    .FindAsync(r => r.RegistrationTime >= DateTime.Now.AddHours(-24));

                activities.AddRange(recentRegistrations.Take(10).Select(r => new RecentActivityDto
                {
                    ActivityType = "Registration",
                    Description = $"New registration: {r.FullName}",
                    UserName = "System",
                    Timestamp = r.RegistrationTime,
                    Icon = "user-plus",
                    Color = "green"
                }));

                // Recent payments
                var recentPayments = recentRegistrations
                    .Where(r => r.PaymentStatus == PaymentStatus.Paid)
                    .Take(5);

                activities.AddRange(recentPayments.Select(r => new RecentActivityDto
                {
                    ActivityType = "Payment",
                    Description = $"Payment received: {r.FullName}",
                    UserName = "System",
                    Timestamp = r.RegistrationTime,
                    Icon = "credit-card",
                    Color = "blue"
                }));

                return activities
                    .OrderByDescending(a => a.Timestamp)
                    .Take(count)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activities");
                throw;
            }
        }

        public async Task AddActivityAsync(string activityType, string description, string userName, Dictionary<string, object>? metadata = null)
        {
            // Log activity - in a real implementation, save to ActivityLog table
            _logger.LogInformation("Activity: {ActivityType} by {UserName}: {Description}",
                activityType, userName, description);
            await Task.CompletedTask;
        }

        public async Task<SystemHealthDto> GetSystemHealthAsync()
        {
            try
            {
                var health = new SystemHealthDto();

                // Email Queue Health
                var emailQueueStatus = await _unitOfWork.EmailQueues.GetStatusCountsAsync();
                health.EmailQueueHealth = new EmailQueueHealthDto
                {
                    PendingCount = emailQueueStatus.GetValueOrDefault(EmailStatus.Pending, 0),
                    ProcessingCount = emailQueueStatus.GetValueOrDefault(EmailStatus.Processing, 0),
                    FailedCount = emailQueueStatus.GetValueOrDefault(EmailStatus.Failed, 0)
                };

                // Determine email queue health status
                var totalEmailIssues = health.EmailQueueHealth.FailedCount +
                    (health.EmailQueueHealth.PendingCount > 100 ? health.EmailQueueHealth.PendingCount - 100 : 0);

                health.EmailQueueHealth.HealthStatus = totalEmailIssues switch
                {
                    0 => "Good",
                    < 10 => "Warning",
                    _ => "Critical"
                };

                // Overall health status
                health.Status = health.EmailQueueHealth.HealthStatus == "Critical" ? "Critical" :
                    health.EmailQueueHealth.HealthStatus == "Warning" ? "Warning" : "Healthy";

                return health;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system health");
                throw;
            }
        }

        public async Task<bool> RunHealthChecksAsync()
        {
            try
            {
                // Database connectivity check - use GetAllAsync instead of CountAsync
                var races = await _unitOfWork.Races.GetAllAsync();
                var raceCount = races.Count();

                // Email queue check - use GetPendingCountAsync if it exists, otherwise use GetAllAsync
                try
                {
                    await _unitOfWork.EmailQueues.GetPendingCountAsync();
                }
                catch
                {
                    // Fallback if GetPendingCountAsync doesn't exist
                    var emails = await _unitOfWork.EmailQueues.GetAllAsync();
                    var pendingCount = emails.Count(e => e.Status == EmailStatus.Pending);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetPerformanceMetricsAsync()
        {
            var metrics = new Dictionary<string, object>();

            try
            {
                // Use GetAllAsync and Count() instead of CountAsync
                var races = await _unitOfWork.Races.GetAllAsync();
                var registrations = await _unitOfWork.Registrations.GetAllAsync();
                var emails = await _unitOfWork.EmailQueues.GetAllAsync();

                var totalRaces = races.Count();
                var totalRegistrations = registrations.Count();
                var pendingEmails = emails.Count(e => e.Status == EmailStatus.Pending);

                metrics.Add("totalRaces", totalRaces);
                metrics.Add("totalRegistrations", totalRegistrations);
                metrics.Add("pendingEmails", pendingEmails);
                metrics.Add("timestamp", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics");
                metrics.Add("error", ex.Message);
            }

            return metrics;
        }

        public async Task<List<RaceAnalyticsDto>> GetTopPerformingRacesAsync(int count = 10)
        {
            try
            {
                var races = await _unitOfWork.Races.GetAllAsync();
                var raceAnalytics = new List<RaceAnalyticsDto>();

                foreach (var race in races.Take(count))
                {
                    var analytics = await GetRaceAnalyticsAsync(race.Id);
                    raceAnalytics.Add(analytics);
                }

                return raceAnalytics
                    .OrderByDescending(r => r.TotalRevenue)
                    .Take(count)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top performing races");
                throw;
            }
        }

        public async Task<List<object>> GetCustomReportAsync(string reportType, Dictionary<string, object> parameters)
        {
            // Implementation for custom reports
            await Task.CompletedTask;
            return new List<object>();
        }

        // Private Helper Methods
        private async Task<EmailDashboardStats> GetEmailDashboardStatsAsync()
        {
            var allEmails = await _unitOfWork.EmailQueues.GetAllAsync();
            var today = DateTime.Today;
            var todayEmails = allEmails.Where(e => e.CreatedAt >= today);

            return new EmailDashboardStats
            {
                TotalEmailsSent = allEmails.Count(e => e.Status == EmailStatus.Sent),
                PendingEmails = allEmails.Count(e => e.Status == EmailStatus.Pending),
                FailedEmails = allEmails.Count(e => e.Status == EmailStatus.Failed),
                EmailsSentToday = todayEmails.Count(e => e.Status == EmailStatus.Sent),
                SuccessRate = CalculateEmailSuccessRate(allEmails),
                EmailTypeBreakdown = allEmails
                    .GroupBy(e => e.EmailType)
                    .Select(g => new EmailTypeStats
                    {
                        EmailType = g.Key.ToString(),
                        Sent = g.Count(e => e.Status == EmailStatus.Sent),
                        Failed = g.Count(e => e.Status == EmailStatus.Failed),
                        Pending = g.Count(e => e.Status == EmailStatus.Pending)
                    })
                    .ToList()
            };
        }

        private double CalculateEmailSuccessRate(IEnumerable<EmailQueue> emails)
        {
            var totalProcessed = emails.Count(e => e.Status == EmailStatus.Sent || e.Status == EmailStatus.Failed);
            var successful = emails.Count(e => e.Status == EmailStatus.Sent);

            return totalProcessed > 0 ? (double)successful / totalProcessed * 100 : 0;
        }

        private List<RegistrationTimelineDto> GetRegistrationTimeline(IEnumerable<Registration> registrations)
        {
            return registrations
                .OrderBy(r => r.RegistrationTime)
                .GroupBy(r => r.RegistrationTime.Date)
                .Select((g, index) => new RegistrationTimelineDto
                {
                    Date = g.Key,
                    DailyRegistrations = g.Count(),
                    CumulativeRegistrations = registrations.Count(r => r.RegistrationTime.Date <= g.Key),
                    CumulativeRevenue = registrations
                        .Where(r => r.RegistrationTime.Date <= g.Key && r.PaymentStatus == PaymentStatus.Paid)
                        .Sum(r => r.Distance?.Price ?? 0)
                })
                .ToList();
        }

        private string CalculateTrendDirection(List<RegistrationTimelineDto> timeline)
        {
            if (timeline.Count < 2) return "stable";

            var recent = timeline.TakeLast(7).Select(t => t.DailyRegistrations).ToList();
            var previous = timeline.SkipLast(7).TakeLast(7).Select(t => t.DailyRegistrations).ToList();

            if (!previous.Any()) return "stable";

            var recentAvg = recent.Average();
            var previousAvg = previous.Average();

            return recentAvg > previousAvg * 1.1 ? "up" :
                   recentAvg < previousAvg * 0.9 ? "down" : "stable";
        }

        private string GetStatusColor(RaceStatus status)
        {
            return status switch
            {
                RaceStatus.Active => "#10B981",      // Green
                RaceStatus.Draft => "#6B7280",       // Gray
                RaceStatus.Completed => "#3B82F6",   // Blue
                RaceStatus.Cancelled => "#EF4444",   // Red
                RaceStatus.Suspended => "#F59E0B",   // Orange
                _ => "#6B7280"
            };
        }
    } // End of DashboardService class
}
