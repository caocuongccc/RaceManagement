using Microsoft.AspNetCore.Mvc;
using RaceManagement.Core.Interfaces;
using RaceManagement.Shared.DTOs;

namespace RaceManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardApiController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardApiController> _logger;

        public DashboardApiController(
            IDashboardService dashboardService,
            ILogger<DashboardApiController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [HttpGet("overview")]
        public async Task<ActionResult<DashboardOverviewDto>> GetOverview()
        {
            try
            {
                var overview = await _dashboardService.GetDashboardOverviewAsync();
                return Ok(overview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard overview");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpGet("overview/daterange")]
        public async Task<ActionResult<DashboardOverviewDto>> GetOverviewByDateRange(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            try
            {
                if (fromDate > toDate)
                {
                    return BadRequest(new { Error = "From date must be before to date" });
                }

                var overview = await _dashboardService.GetDashboardOverviewAsync(fromDate, toDate);
                return Ok(overview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard overview for date range");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpGet("race/{raceId}/analytics")]
        public async Task<ActionResult<RaceAnalyticsDto>> GetRaceAnalytics(int raceId)
        {
            try
            {
                var analytics = await _dashboardService.GetRaceAnalyticsAsync(raceId);
                return Ok(analytics);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting race analytics for race {RaceId}", raceId);
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpGet("trends/registrations")]
        public async Task<ActionResult<List<RegistrationTrendDto>>> GetRegistrationTrends(
            [FromQuery] int days = 30)
        {
            try
            {
                var trends = await _dashboardService.GetRegistrationTrendsAsync(days);
                return Ok(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting registration trends");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpGet("trends/revenue")]
        public async Task<ActionResult<List<RevenueTrendDto>>> GetRevenueTrends(
            [FromQuery] int days = 30)
        {
            try
            {
                var trends = await _dashboardService.GetRevenueTrendsAsync(days);
                return Ok(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue trends");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpGet("race-status-distribution")]
        public async Task<ActionResult<List<RaceStatusDistributionDto>>> GetRaceStatusDistribution()
        {
            try
            {
                var distribution = await _dashboardService.GetRaceStatusDistributionAsync();
                return Ok(distribution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting race status distribution");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpGet("activities/recent")]
        public async Task<ActionResult<List<RecentActivityDto>>> GetRecentActivities(
            [FromQuery] int count = 20)
        {
            try
            {
                var activities = await _dashboardService.GetRecentActivitiesAsync(count);
                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent activities");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpGet("health")]
        public async Task<ActionResult<SystemHealthDto>> GetSystemHealth()
        {
            try
            {
                var health = await _dashboardService.GetSystemHealthAsync();
                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system health");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpPost("health/check")]
        public async Task<ActionResult> RunHealthChecks()
        {
            try
            {
                var result = await _dashboardService.RunHealthChecksAsync();
                return Ok(new { Success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running health checks");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpGet("metrics/performance")]
        public async Task<ActionResult<Dictionary<string, object>>> GetPerformanceMetrics()
        {
            try
            {
                var metrics = await _dashboardService.GetPerformanceMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpGet("races/top-performing")]
        public async Task<ActionResult<List<RaceAnalyticsDto>>> GetTopPerformingRaces(
            [FromQuery] int count = 10)
        {
            try
            {
                var races = await _dashboardService.GetTopPerformingRacesAsync(count);
                return Ok(races);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top performing races");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpPost("activities")]
        public async Task<ActionResult> AddActivity([FromBody] AddActivityRequest request)
        {
            try
            {
                await _dashboardService.AddActivityAsync(
                    request.ActivityType,
                    request.Description,
                    request.UserName,
                    request.Metadata);

                return Ok(new { Success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding activity");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpPost("reports/custom")]
        public async Task<ActionResult<List<object>>> GetCustomReport([FromBody] CustomReportRequest request)
        {
            try
            {
                var report = await _dashboardService.GetCustomReportAsync(request.ReportType, request.Parameters);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating custom report");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }
    }

    // Request DTOs for Dashboard Controller
    public class AddActivityRequest
    {
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class CustomReportRequest
    {
        public string ReportType { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
    }
}
