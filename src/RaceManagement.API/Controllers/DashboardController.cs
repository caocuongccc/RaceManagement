using Microsoft.AspNetCore.Mvc;
using RaceManagement.API.Extensions;
using RaceManagement.API.ViewModels;
using RaceManagement.Core.Interfaces;
using RaceManagement.Shared.DTOs;

namespace RaceManagement.API.Controllers
{
    public class DashboardController : Controller // MVC Controller (not API)
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardService dashboardService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        // Main Dashboard Page - Updated to use ViewModel
        public async Task<IActionResult> Index()
        {
            try
            {
                var overview = await _dashboardService.GetDashboardOverviewAsync();
                var viewModel = overview.ToDashboardViewModel();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                var errorViewModel = new DashboardViewModel
                {
                    HasErrors = true,
                    ErrorMessage = "Error loading dashboard data"
                };
                return View(errorViewModel);
            }
        }

        // Race Analytics Page - Updated
        public async Task<IActionResult> RaceAnalytics(int id)
        {
            try
            {
                var analytics = await _dashboardService.GetRaceAnalyticsAsync(id);
                var trends = await _dashboardService.GetRegistrationTrendsAsync(30);

                var viewModel = analytics.ToRaceAnalyticsViewModel();
                viewModel.RegistrationTrends = trends;

                return View(viewModel);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading race analytics for race {RaceId}", id);
                return View(new RaceAnalyticsViewModel());
            }
        }

        // System Health Page - Updated
        public async Task<IActionResult> SystemHealth()
        {
            try
            {
                var health = await _dashboardService.GetSystemHealthAsync();
                var viewModel = health.ToSystemHealthViewModel();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading system health");
                return View(new SystemHealthViewModel());
            }
        }

        // Reports Page - Updated
        public async Task<IActionResult> Reports()
        {
            try
            {
                var topRaces = await _dashboardService.GetTopPerformingRacesAsync(10);
                var trends = await _dashboardService.GetRegistrationTrendsAsync(90);
                var revenueTrends = await _dashboardService.GetRevenueTrendsAsync(90);

                var viewModel = new ReportsViewModel
                {
                    TopPerformingRaces = topRaces,
                    OverallTrends = trends,
                    RevenueTrends = revenueTrends
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reports");
                return View(new ReportsViewModel());
            }
        }

        // Partial View Actions
        public async Task<IActionResult> StatsPartial()
        {
            try
            {
                var overview = await _dashboardService.GetDashboardOverviewAsync();
                return PartialView("_StatsPartial", overview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stats");
                return PartialView("_ErrorPartial", "Error loading statistics");
            }
        }

        public async Task<IActionResult> RecentActivitiesPartial(int count = 10)
        {
            try
            {
                var activities = await _dashboardService.GetRecentActivitiesAsync(count);
                return PartialView("_RecentActivitiesPartial", activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading activities");
                return PartialView("_ErrorPartial", "Error loading activities");
            }
        }

        public async Task<IActionResult> EmailStatsPartial()
        {
            try
            {
                var overview = await _dashboardService.GetDashboardOverviewAsync();
                return PartialView("_EmailStatsPartial", overview.EmailStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading email stats");
                return PartialView("_ErrorPartial", "Error loading email statistics");
            }
        }

        // AJAX Endpoints - Return JSON for charts
        [HttpGet]
        public async Task<JsonResult> GetRegistrationTrends(int days = 30)
        {
            try
            {
                var trends = await _dashboardService.GetRegistrationTrendsAsync(days);
                var chartData = trends.ToRegistrationChartData();
                return Json(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting registration trends");
                return Json(new { error = "Error loading data" });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetRevenueTrends(int days = 30)
        {
            try
            {
                var trends = await _dashboardService.GetRevenueTrendsAsync(days);
                var chartData = trends.ToRevenueChartData();
                return Json(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue trends");
                return Json(new { error = "Error loading data" });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetRaceStatusDistribution()
        {
            try
            {
                var distribution = await _dashboardService.GetRaceStatusDistributionAsync();
                return Json(distribution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting race status distribution");
                return Json(new { error = "Error loading data" });
            }
        }

        // Health Check
        [HttpPost]
        public async Task<JsonResult> RunHealthCheck()
        {
            try
            {
                var result = await _dashboardService.RunHealthChecksAsync();
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return Json(new { success = false, error = ex.Message });
            }
        }

        // Export functionality
        public async Task<IActionResult> ExportDashboardData(string format = "json")
        {
            try
            {
                var overview = await _dashboardService.GetDashboardOverviewAsync();

                switch (format.ToLower())
                {
                    case "json":
                        return Json(overview);
                    case "csv":
                        // Could implement CSV export here
                        return BadRequest("CSV export not implemented yet");
                    default:
                        return BadRequest("Unsupported format");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting dashboard data");
                return StatusCode(500, "Error exporting data");
            }
        }
    }
}
