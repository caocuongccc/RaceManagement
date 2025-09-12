using Microsoft.AspNetCore.Mvc;
using RaceManagement.Application.Services;
using RaceManagement.Shared.DTOs;

namespace RaceManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SheetConfigsController : ControllerBase
    {
        private readonly ISheetConfigService _sheetConfigService;
        private readonly ILogger<SheetConfigsController> _logger;

        public SheetConfigsController(ISheetConfigService sheetConfigService, ILogger<SheetConfigsController> logger)
        {
            _sheetConfigService = sheetConfigService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả sheet configs
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SheetConfigDto>>> GetAllSheetConfigs([FromQuery] string? search = null)
        {
            try
            {
                var configs = string.IsNullOrEmpty(search)
                    ? await _sheetConfigService.GetAllSheetConfigsAsync()
                    : await _sheetConfigService.SearchSheetConfigsAsync(search);

                return Ok(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sheet configs with search term: {SearchTerm}", search);
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy danh sách sheet configs đang hoạt động
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<SheetConfigDto>>> GetActiveSheetConfigs()
        {
            try
            {
                var configs = await _sheetConfigService.GetActiveSheetConfigsAsync();
                return Ok(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sheet configs");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy danh sách sheet configs cho dropdown selection
        /// </summary>
        [HttpGet("select-list")]
        public async Task<ActionResult<IEnumerable<SheetConfigSelectDto>>> GetSheetConfigSelectList()
        {
            try
            {
                var selectList = await _sheetConfigService.GetSheetConfigSelectListAsync();
                return Ok(selectList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sheet config select list");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy sheet configs theo credential ID
        /// </summary>
        [HttpGet("by-credential/{credentialId}")]
        public async Task<ActionResult<IEnumerable<SheetConfigDto>>> GetConfigsByCredential(int credentialId)
        {
            try
            {
                var configs = await _sheetConfigService.GetConfigsByCredentialAsync(credentialId);
                return Ok(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sheet configs for credential {CredentialId}", credentialId);
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy sheet configs select list theo credential ID
        /// </summary>
        [HttpGet("select-list/by-credential/{credentialId}")]
        public async Task<ActionResult<IEnumerable<SheetConfigSelectDto>>> GetSheetConfigSelectByCredential(int credentialId)
        {
            try
            {
                var selectList = await _sheetConfigService.GetSheetConfigSelectByCredentialAsync(credentialId);
                return Ok(selectList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sheet config select list for credential {CredentialId}", credentialId);
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy thông tin sheet config theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<SheetConfigDto>> GetSheetConfig(int id)
        {
            try
            {
                var config = await _sheetConfigService.GetSheetConfigAsync(id);
                if (config == null)
                {
                    return NotFound(new { Error = $"Sheet config with ID {id} not found" });
                }

                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sheet config {SheetConfigId}", id);
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Tạo sheet config mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SheetConfigDto>> CreateSheetConfig([FromBody] CreateSheetConfigDto dto)
        {
            try
            {
                var config = await _sheetConfigService.CreateSheetConfigAsync(dto);
                return CreatedAtAction(nameof(GetSheetConfig), new { id = config.Id }, config);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sheet config: {SheetConfigName}", dto.Name);
                return StatusCode(500, new { Error = "Failed to create sheet config" });
            }
        }

        /// <summary>
        /// Cập nhật sheet config
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<SheetConfigDto>> UpdateSheetConfig(int id, [FromBody] UpdateSheetConfigDto dto)
        {
            try
            {
                var config = await _sheetConfigService.UpdateSheetConfigAsync(id, dto);
                return Ok(config);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sheet config {SheetConfigId}", id);
                return StatusCode(500, new { Error = "Failed to update sheet config" });
            }
        }

        /// <summary>
        /// Xóa sheet config
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSheetConfig(int id)
        {
            try
            {
                await _sheetConfigService.DeleteSheetConfigAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sheet config {SheetConfigId}", id);
                return StatusCode(500, new { Error = "Failed to delete sheet config" });
            }
        }

        /// <summary>
        /// Test kết nối sheet config
        /// </summary>
        [HttpPost("{id}/test")]
        public async Task<ActionResult<SheetConfigTestResult>> TestSheetConnection(int id)
        {
            try
            {
                var result = await _sheetConfigService.TestSheetConnectionAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing sheet config {SheetConfigId}", id);
                return StatusCode(500, new { Error = "Failed to test sheet connection" });
            }
        }

        /// <summary>
        /// Lấy metadata của Google Sheet
        /// </summary>
        [HttpGet("{id}/metadata")]
        public async Task<ActionResult<SheetMetadataDto>> GetSheetMetadata(int id)
        {
            try
            {
                var metadata = await _sheetConfigService.GetSheetMetadataAsync(id);
                return Ok(metadata);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sheet metadata for config {SheetConfigId}", id);
                return StatusCode(500, new { Error = "Failed to get sheet metadata" });
            }
        }

        /// <summary>
        /// Lấy danh sách tên các sheet trong spreadsheet
        /// </summary>
        [HttpGet("{id}/sheet-names")]
        public async Task<ActionResult<IEnumerable<string>>> GetSheetNames(int id)
        {
            try
            {
                var sheetNames = await _sheetConfigService.GetSheetNamesAsync(id);
                return Ok(sheetNames);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sheet names for config {SheetConfigId}", id);
                return StatusCode(500, new { Error = "Failed to get sheet names" });
            }
        }

        /// <summary>
        /// Cập nhật last sync row cho sheet config
        /// </summary>
        [HttpPut("{id}/last-sync-row")]
        public async Task<IActionResult> UpdateLastSyncRow(int id, [FromBody] UpdateLastSyncRowRequest request)
        {
            try
            {
                await _sheetConfigService.UpdateLastSyncRowAsync(id, request.RowIndex);
                return Ok(new { Message = "Last sync row updated successfully", RowIndex = request.RowIndex });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last sync row for config {SheetConfigId}", id);
                return StatusCode(500, new { Error = "Failed to update last sync row" });
            }
        }

        /// <summary>
        /// Lấy next sync row cho sheet config
        /// </summary>
        [HttpGet("{id}/next-sync-row")]
        public async Task<ActionResult<int>> GetNextSyncRow(int id)
        {
            try
            {
                var nextRow = await _sheetConfigService.GetNextSyncRowAsync(id);
                return Ok(new { NextSyncRow = nextRow });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next sync row for config {SheetConfigId}", id);
                return StatusCode(500, new { Error = "Failed to get next sync row" });
            }
        }

        /// <summary>
        /// Bulk operations trên multiple sheet configs
        /// </summary>
        [HttpPost("bulk")]
        public async Task<ActionResult<BulkOperationResult>> BulkOperation([FromBody] BulkSheetConfigOperation operation)
        {
            try
            {
                if (!operation.SheetConfigIds.Any())
                {
                    return BadRequest(new { Error = "No sheet config IDs provided" });
                }

                var validOperations = new[] { "activate", "deactivate", "test" };
                if (!validOperations.Contains(operation.Operation.ToLower()))
                {
                    return BadRequest(new
                    {
                        Error = "Invalid operation",
                        ValidOperations = validOperations
                    });
                }

                var result = await _sheetConfigService.BulkUpdateStatusAsync(operation);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk operation: {Operation}", operation.Operation);
                return StatusCode(500, new { Error = "Bulk operation failed" });
            }
        }
    }
}
