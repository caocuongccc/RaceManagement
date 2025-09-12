using Microsoft.AspNetCore.Mvc;
using RaceManagement.Application.Services;
using RaceManagement.Core.Models;
using RaceManagement.Shared.DTOs;

namespace RaceManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CredentialsController : ControllerBase
    {
        private readonly ICredentialService _credentialService;
        private readonly ILogger<CredentialsController> _logger;

        public CredentialsController(ICredentialService credentialService, ILogger<CredentialsController> logger)
        {
            _credentialService = credentialService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả credentials
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CredentialDto>>> GetAllCredentials([FromQuery] string? search = null)
        {
            try
            {
                var credentials = string.IsNullOrEmpty(search)
                    ? await _credentialService.GetAllCredentialsAsync()
                    : await _credentialService.SearchCredentialsAsync(search);

                return Ok(credentials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting credentials with search term: {SearchTerm}", search);
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy danh sách credentials đang hoạt động
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<CredentialDto>>> GetActiveCredentials()
        {
            try
            {
                var credentials = await _credentialService.GetActiveCredentialsAsync();
                return Ok(credentials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active credentials");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy danh sách credentials cho dropdown selection
        /// </summary>
        [HttpGet("select-list")]
        public async Task<ActionResult<IEnumerable<CredentialSelectDto>>> GetCredentialSelectList()
        {
            try
            {
                var selectList = await _credentialService.GetCredentialSelectListAsync();
                return Ok(selectList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting credential select list");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy thông tin credential theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CredentialDto>> GetCredential(int id)
        {
            try
            {
                var credential = await _credentialService.GetCredentialAsync(id);
                if (credential == null)
                {
                    return NotFound(new { Error = $"Credential with ID {id} not found" });
                }

                return Ok(credential);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting credential {CredentialId}", id);
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Tạo credential mới với upload file JSON
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CredentialDto>> CreateCredential([FromForm] CreateCredentialDto dto)
        {
            try
            {
                // Validate file trước khi xử lý
                var fileValidation = await _credentialService.ValidateCredentialFileAsync(dto.CredentialFile);
                if (!fileValidation.IsValid)
                {
                    return BadRequest(new
                    {
                        Error = "Invalid credential file",
                        Details = fileValidation.Errors
                    });
                }

                var credential = await _credentialService.CreateCredentialAsync(dto);
                return CreatedAtAction(nameof(GetCredential), new { id = credential.Id }, credential);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating credential: {CredentialName}", dto.Name);
                return StatusCode(500, new { Error = "Failed to create credential" });
            }
        }

        /// <summary>
        /// Cập nhật credential
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<CredentialDto>> UpdateCredential(int id, [FromForm] UpdateCredentialDto dto)
        {
            try
            {
                // Validate file nếu có upload file mới
                if (dto.CredentialFile != null)
                {
                    var fileValidation = await _credentialService.ValidateCredentialFileAsync(dto.CredentialFile);
                    if (!fileValidation.IsValid)
                    {
                        return BadRequest(new
                        {
                            Error = "Invalid credential file",
                            Details = fileValidation.Errors
                        });
                    }
                }

                var credential = await _credentialService.UpdateCredentialAsync(id, dto);
                return Ok(credential);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating credential {CredentialId}", id);
                return StatusCode(500, new { Error = "Failed to update credential" });
            }
        }

        /// <summary>
        /// Xóa credential
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCredential(int id)
        {
            try
            {
                await _credentialService.DeleteCredentialAsync(id);
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
                _logger.LogError(ex, "Error deleting credential {CredentialId}", id);
                return StatusCode(500, new { Error = "Failed to delete credential" });
            }
        }

        /// <summary>
        /// Test kết nối credential
        /// </summary>
        [HttpPost("{id}/test")]
        public async Task<ActionResult<CredentialTestResult>> TestCredential(int id)
        {
            try
            {
                var result = await _credentialService.TestCredentialAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing credential {CredentialId}", id);
                return StatusCode(500, new { Error = "Failed to test credential" });
            }
        }

        /// <summary>
        /// Validate credential file trước khi upload
        /// </summary>
        [HttpPost("validate-file")]
        public async Task<ActionResult<CredentialValidationResult>> ValidateCredentialFile(IFormFile file)
        {
            try
            {
                var result = await _credentialService.ValidateCredentialFileAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating credential file");
                return StatusCode(500, new { Error = "Failed to validate file" });
            }
        }

        /// <summary>
        /// Bulk operations trên multiple credentials
        /// </summary>
        [HttpPost("bulk")]
        public async Task<ActionResult<BulkOperationResult>> BulkOperation([FromBody] BulkCredentialOperation operation)
        {
            try
            {
                if (!operation.CredentialIds.Any())
                {
                    return BadRequest(new { Error = "No credential IDs provided" });
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

                var result = await _credentialService.BulkUpdateStatusAsync(operation);
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
