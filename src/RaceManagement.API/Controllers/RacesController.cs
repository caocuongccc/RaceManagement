using Microsoft.AspNetCore.Mvc;
using RaceManagement.Application.Services;
using RaceManagement.Shared.DTOs;

namespace RaceManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RacesController : ControllerBase
    {
        private readonly IRaceService _raceService;
        private readonly ILogger<RacesController> _logger;

        public RacesController(IRaceService raceService, ILogger<RacesController> logger)
        {
            _raceService = raceService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo giải chạy mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RaceDto>> CreateRace([FromBody] CreateRaceDto dto)
        {
            try
            {
                var race = await _raceService.CreateRaceAsync(dto);
                return CreatedAtAction(nameof(GetRace), new { id = race.Id }, race);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create race: {RaceName}", dto.Name);
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin giải chạy
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RaceDto>> GetRace(int id)
        {
            var race = await _raceService.GetRaceAsync(id);
            return race != null ? Ok(race) : NotFound();
        }

        /// <summary>
        /// Lấy danh sách giải chạy đang hoạt động
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RaceDto>>> GetActiveRaces()
        {
            var races = await _raceService.GetActiveRacesAsync();
            return Ok(races);
        }

        /// <summary>
        /// Lấy thống kê giải chạy
        /// </summary>
        [HttpGet("{id}/statistics")]
        public async Task<ActionResult<RaceStatisticsDto>> GetStatistics(int id)
        {
            var stats = await _raceService.GetRaceStatisticsAsync(id);
            return Ok(stats);
        }
    }
}
