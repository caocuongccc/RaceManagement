using Microsoft.EntityFrameworkCore;
using RaceManagement.Shared.Enums;
using RaceManagement.Core.Interfaces;
using RaceManagement.Infrastructure.Data;
using RaceManagement.Shared.DTOs;

namespace RaceManagement.Infrastructure.Repositories
{
    // CORRECTED: Follow same pattern as EmailQueueRepository
    public class DashboardRepository : IDashboardRepository
    {
        private readonly RaceManagementDbContext _context;

        public DashboardRepository(RaceManagementDbContext context) // Single parameter like other repos
        {
            _context = context;
        }

        public async Task<Dictionary<DateTime, int>> GetRegistrationTrendDataAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Registrations
                .Where(r => r.RegistrationTime >= fromDate && r.RegistrationTime <= toDate)
                .GroupBy(r => r.RegistrationTime.Date)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<DateTime, decimal>> GetRevenueTrendDataAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Registrations
                .Include(r => r.Distance)
                .Where(r => r.PaymentStatus == PaymentStatus.Paid &&
                           r.RegistrationTime >= fromDate &&
                           r.RegistrationTime <= toDate)
                .GroupBy(r => r.RegistrationTime.Date)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(r => r.Distance.Price));
        }

        public async Task<List<RacePerformanceData>> GetRacePerformanceDataAsync(int topCount = 10)
        {
            return await _context.Races
                .Include(r => r.Registrations)
                    .ThenInclude(reg => reg.Distance)
                .Select(r => new RacePerformanceData
                {
                    RaceId = r.Id,
                    RaceName = r.Name,
                    RaceDate = r.RaceDate,
                    TotalRegistrations = r.Registrations.Count(),
                    TotalRevenue = r.Registrations
                        .Where(reg => reg.PaymentStatus == PaymentStatus.Paid)
                        .Sum(reg => reg.Distance.Price),
                    CompletionRate = r.Registrations.Any() ?
                        (double)r.Registrations.Count(reg => reg.PaymentStatus == PaymentStatus.Paid) /
                        r.Registrations.Count() * 100 : 0
                })
                .OrderByDescending(r => r.TotalRevenue)
                .Take(topCount)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetRaceStatusCountsAsync()
        {
            return await _context.Races
                .GroupBy(r => r.Status)
                .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());
        }

        public async Task<List<ActivityLogEntry>> GetRecentActivitiesAsync(int count = 20)
        {
            // FIXED: Avoid complex metadata in LINQ to Entities
            var recentRegistrations = await _context.Registrations
                .Include(r => r.Race)
                .Include(r => r.Distance)
                .OrderByDescending(r => r.RegistrationTime)
                .Take(count)
                .ToListAsync(); // Materialize first

            // Then create ActivityLogEntry in memory
            var activities = recentRegistrations.Select(r => new ActivityLogEntry
            {
                ActivityType = "Registration",
                Description = $"New registration for {r.Race.Name}: {r.FullName}",
                Timestamp = r.RegistrationTime,
                UserId = "System",
                Metadata = new Dictionary<string, object> // This works in memory
                {
                    { "RaceId", r.RaceId },
                    { "RegistrationId", r.Id },
                    { "Amount", r.Distance?.Price ?? 0 }
                }
            }).ToList();

            return activities;
        }

        public async Task<SystemMetrics> GetSystemMetricsAsync()
        {
            var metrics = new SystemMetrics
            {
                TotalRaces = await _context.Races.CountAsync(),
                TotalRegistrations = await _context.Registrations.CountAsync(),
                TotalPendingEmails = await _context.EmailQueues.CountAsync(e => e.Status == EmailStatus.Pending),
                TotalFailedEmails = await _context.EmailQueues.CountAsync(e => e.Status == EmailStatus.Failed),
                DatabaseSize = await GetDatabaseSizeAsync(),
                LastCalculated = DateTime.UtcNow
            };

            return metrics;
        }

        private async Task<long> GetDatabaseSizeAsync()
        {
            try
            {
                // Simplified - return 0 if can't calculate
                return 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}