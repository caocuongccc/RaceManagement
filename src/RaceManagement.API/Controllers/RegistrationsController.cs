using Microsoft.AspNetCore.Mvc;
using RaceManagement.Application.Services;
using RaceManagement.Shared.DTOs;

namespace RaceManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationsController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        private readonly ILogger<RegistrationsController> _logger;

        public RegistrationsController(
            IRegistrationService registrationService,
            ILogger<RegistrationsController> logger)
        {
            _registrationService = registrationService;
            _logger = logger;
        }

        /// <summary>
        /// Sync đăng ký từ Google Sheet
        /// </summary>
        [HttpPost("sync/{raceId}")]
        public async Task<ActionResult<SyncResultDto>> SyncRegistrations(int raceId)
        {
            var result = await _registrationService.SyncRegistrationsFromSheetAsync(raceId);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        /// <summary>
        /// Lấy danh sách đăng ký theo giải chạy
        /// </summary>
        [HttpGet("race/{raceId}")]
        public async Task<ActionResult<IEnumerable<RegistrationDto>>> GetRegistrationsByRace(int raceId)
        {
            var registrations = await _registrationService.GetRegistrationsByRaceAsync(raceId);
            return Ok(registrations);
        }

        /// <summary>
        /// Lấy thông tin đăng ký
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RegistrationDto>> GetRegistration(int id)
        {
            var registration = await _registrationService.GetRegistrationAsync(id);
            return registration != null ? Ok(registration) : NotFound();
        }

        /// <summary>
        /// Xử lý thanh toán thủ công
        /// </summary>
        [HttpPost("{id}/payment")]
        public async Task<IActionResult> ProcessPayment(int id, [FromBody] PaymentNotificationDto notification)
        {
            try
            {
                await _registrationService.ProcessPaymentAsync(id, notification);
                return Ok(new { Message = "Payment processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process payment for registration {RegistrationId}", id);
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Generate mã tham chiếu mới
        /// </summary>
        [HttpGet("generate-reference")]
        public async Task<ActionResult<string>> GenerateReference()
        {
            var reference = await _registrationService.GenerateTransactionReference();
            return Ok(new { TransactionReference = reference });
        }
    }
}
