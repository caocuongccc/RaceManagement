using Microsoft.AspNetCore.Mvc;
using RaceManagement.Core.Interfaces;

namespace RaceManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoogleSheetsTestController : ControllerBase
    {
        private readonly IGoogleSheetsService _googleSheetsService;

        public GoogleSheetsTestController(IGoogleSheetsService googleSheetsService)
        {
            _googleSheetsService = googleSheetsService;
        }

        [HttpGet("test-connection/{sheetId}")]
        public async Task<IActionResult> TestConnection(string sheetId)
        {
            var isConnected = await _googleSheetsService.TestConnectionAsync(sheetId);
            return Ok(new { SheetId = sheetId, IsConnected = isConnected });
        }

        [HttpGet("read-registrations/{sheetId}")]
        public async Task<IActionResult> ReadRegistrations(string sheetId, [FromQuery] int fromRow = 1)
        {
            try
            {
                var registrations = await _googleSheetsService.ReadNewRegistrationsAsync(sheetId, fromRow);
                return Ok(new
                {
                    SheetId = sheetId,
                    FromRow = fromRow,
                    Count = registrations.Count(),
                    Data = registrations
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("total-rows/{sheetId}")]
        public async Task<IActionResult> GetTotalRows(string sheetId)
        {
            var totalRows = await _googleSheetsService.GetTotalRowsAsync(sheetId);
            return Ok(new { SheetId = sheetId, TotalRows = totalRows });
        }
    }
}
