using Microsoft.AspNetCore.Mvc;
using RaceManagement.Core.Interfaces;

namespace RaceManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public HealthController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Kiểm tra API có hoạt động không
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                Status = "OK",
                Timestamp = DateTime.Now,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            });
        }

        /// <summary>
        /// Kiểm tra database connection
        /// </summary>
        [HttpGet("database")]
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                // Test basic database connection
                var raceCount = await _unitOfWork.Races.CountAsync(r => true);
                var registrationCount = await _unitOfWork.Registrations.CountAsync(r => true);

                return Ok(new
                {
                    Status = "Connected",
                    DatabaseName = "RaceManagement",
                    TotalRaces = raceCount,
                    TotalRegistrations = registrationCount,
                    ConnectionTime = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Failed",
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Get application info
        /// </summary>
        [HttpGet("info")]
        public IActionResult GetInfo()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version?.ToString() ?? "Unknown";

            return Ok(new
            {
                ApplicationName = "Race Management System",
                Version = version,
                BuildDate = DateTime.Now.ToString("yyyy-MM-dd"),
                Framework = ".NET 8",
                Database = "SQL Server"
            });
        }
    }
}
