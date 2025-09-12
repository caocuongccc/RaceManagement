using Hangfire;
using Microsoft.AspNetCore.Mvc;
using RaceManagement.Application.Jobs;
using RaceManagement.Core.Interfaces;
using RaceManagement.Shared.DTOs;
using RaceManagement.Abstractions.Enums;
using RaceManagement.Core.Entities;
using RaceManagement.Infrastructure.Repositories;

namespace RaceManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailsController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IEmailJob _emailJob;
        private readonly ILogger<EmailsController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public EmailsController(
            IEmailService emailService,
            IEmailJob emailJob,
            ILogger<EmailsController> logger,
            IUnitOfWork unitOfWork)
        {
            _emailService = emailService;
            _emailJob = emailJob;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Send registration confirmation email immediately
        /// </summary>
        [HttpPost("registration-confirmation/{registrationId}")]
        public async Task<ActionResult<EmailResult>> SendRegistrationConfirmation(int registrationId)
        {
            try
            {
                var result = await _emailService.SendRegistrationConfirmationAsync(registrationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending registration confirmation for {RegistrationId}", registrationId);
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Send BIB notification email immediately
        /// </summary>
        [HttpPost("bib-notification/{registrationId}")]
        public async Task<ActionResult<EmailResult>> SendBibNotification(int registrationId)
        {
            try
            {
                var result = await _emailService.SendBibNotificationAsync(registrationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending BIB notification for {RegistrationId}", registrationId);
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Send payment reminder email immediately
        /// </summary>
        [HttpPost("payment-reminder/{registrationId}")]
        public async Task<ActionResult<EmailResult>> SendPaymentReminder(int registrationId)
        {
            try
            {
                var result = await _emailService.SendPaymentReminderAsync(registrationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment reminder for {RegistrationId}", registrationId);
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Send race day info email immediately
        /// </summary>
        [HttpPost("race-day-info/{registrationId}")]
        public async Task<ActionResult<EmailResult>> SendRaceDayInfo(int registrationId)
        {
            try
            {
                var result = await _emailService.SendRaceDayInfoAsync(registrationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending race day info for {RegistrationId}", registrationId);
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Queue email for later processing
        /// </summary>
        [HttpPost("queue")]
        public async Task<IActionResult> QueueEmail([FromBody] QueueEmailRequest request)
        {
            try
            {
                await _emailService.QueueEmailAsync(request.RegistrationId, request.EmailType, request.ScheduledAt);

                var jobId = request.ScheduledAt.HasValue
                    ? BackgroundJob.Schedule<IEmailJob>(x => x.ProcessScheduledEmailsAsync(), request.ScheduledAt.Value)
                    : BackgroundJob.Enqueue<IEmailJob>(x => x.ProcessPendingEmailsAsync());

                return Ok(new { Message = "Email queued successfully", JobId = jobId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error queueing email for registration {RegistrationId}", request.RegistrationId);
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Send bulk emails for a race
        /// </summary>
        [HttpPost("bulk/{raceId}")]
        public ActionResult<BulkEmailResult> SendBulkEmails(int raceId, [FromBody] BulkEmailRaceRequest request)
        {
            try
            {
                //var jobId = 0;
                switch (request.EmailType)
                {
                    case EmailType.PaymentReminder:
                        BackgroundJob.Enqueue<IEmailJob>(x => x.SendPaymentReminderEmailsAsync(raceId));
                        break;

                    case EmailType.RaceDayInfo:
                        BackgroundJob.Enqueue<IEmailJob>(x => x.SendRaceDayInfoEmailsAsync(raceId));
                        break;

                    default:
                        BackgroundJob.Enqueue<IEmailJob>(x => x.ProcessPendingEmailsAsync());
                        break;
                }
                //var jobId = BackgroundJob.Enqueue<IEmailJob>(x =>
                //    request.EmailType switch
                //    {
                //        EmailType.PaymentReminder => x.SendPaymentReminderEmailsAsync(raceId),
                //        EmailType.RaceDayInfo => x.SendRaceDayInfoEmailsAsync(raceId),
                //        _ => x.ProcessPendingEmailsAsync()
                //    });
                
                return Ok(new { Message = "Bulk email job queued", JobId = raceId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error queueing bulk emails for race {RaceId}", raceId);
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Get email queue status
        /// </summary>
        [HttpGet("queue/status")]
        public async Task<ActionResult<EmailQueueStatusDto>> GetQueueStatus()
        {
            try
            {
                var status = await _emailService.GetQueueStatusAsync();
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting queue status");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Trigger manual processing of pending emails
        /// </summary>
        [HttpPost("process-pending")]
        public IActionResult ProcessPendingEmails()
        {
            try
            {
                var jobId = BackgroundJob.Enqueue<IEmailJob>(x => x.ProcessPendingEmailsAsync());
                return Ok(new { Message = "Email processing job queued", JobId = jobId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering email processing");
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Retry failed emails
        /// </summary>
        [HttpPost("retry-failed")]
        public IActionResult RetryFailedEmails()
        {
            try
            {
                var jobId = BackgroundJob.Enqueue<IEmailJob>(x => x.RetryFailedEmailsAsync());
                return Ok(new { Message = "Retry failed emails job queued", JobId = jobId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering retry failed emails");
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Test email configuration
        /// </summary>
        [HttpPost("test")]
        public async Task<ActionResult<EmailResult>> TestEmail([FromBody] TestEmailRequest request)
        {
            try
            {
                var emailRequest = new EmailRequest
                {
                    To = request.To,
                    ToName = request.ToName,
                    Subject = "Test Email - Race Management System",
                    HtmlContent = """
                    <html>
                    <body style="font-family: Arial, sans-serif;">
                        <h1>Test Email</h1>
                        <p>This is a test email from the Race Management System.</p>
                        <p>If you receive this email, the email configuration is working correctly.</p>
                        <p>Sent at: {{DateTime.Now}}</p>
                    </body>
                    </html>
                    """.Replace("{{DateTime.Now}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                    Priority = EmailPriority.Normal
                };

                var result = await _emailService.SendEmailAsync(emailRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test email");
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Get available email templates
        /// </summary>
        [HttpGet("templates")]
        public ActionResult<IEnumerable<string>> GetTemplates()
        {
            try
            {
                // This would require adding GetAvailableTemplatesAsync to IEmailService
                var templates = new[] { "registration-confirmation", "bib-notification", "payment-reminder", "race-day-info" };
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting email templates");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        /// <summary>
        /// Preview email template
        /// </summary>
        [HttpPost("templates/{templateName}/preview")]
        public async Task<ActionResult<string>> PreviewTemplate(string templateName, [FromBody] int registrationId)
        {
            try
            {
                // Get registration for preview data
                var registration = await _unitOfWork.Registrations.GetByIdWithIncludesAsync(registrationId);
                if (registration == null)
                {
                    return NotFound("Registration not found");
                }

                var registrationDto = MapToRegistrationDto(registration);
                var htmlContent = await _emailService.RenderTemplateAsync(templateName, registrationDto);

                return Ok(new { HtmlContent = htmlContent, Subject = GetTemplateSubject(templateName, registrationDto) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing template {TemplateName}", templateName);
                return BadRequest(new { Error = ex.Message });
            }

        }

        // Helper methods
        private RegistrationDto MapToRegistrationDto(Registration registration)
        {
            return new RegistrationDto
            {
                Id = registration.Id,
                RaceId = registration.RaceId,
                RaceName = registration.Race.Name,
                DistanceId = registration.DistanceId,
                Distance = registration.Distance.Distance,
                Price = registration.Distance.Price,
                FullName = registration.FullName,
                BibName = registration.BibName,
                Email = registration.Email,
                Phone = registration.Phone,
                BirthYear = registration.BirthYear,
                DateOfBirth = registration.DateOfBirth,
                Gender = registration.Gender?.ToString(),
                ShirtCategory = registration.ShirtCategory,
                ShirtSize = registration.ShirtSize,
                ShirtType = registration.ShirtType,
                EmergencyContact = registration.EmergencyContact,
                RegistrationTime = registration.RegistrationTime,
                PaymentStatus = registration.PaymentStatus.ToString(),
                BibNumber = registration.BibNumber,
                BibSentAt = registration.BibSentAt,
                TransactionReference = registration.TransactionReference
            };
        }

        private string GetTemplateSubject(string templateName, RegistrationDto registration)
        {
            return templateName.ToLower() switch
            {
                "registration-confirmation" => $"Xác nhận đăng ký {registration.RaceName} - {registration.FullName}",
                "bib-notification" => $"Số BIB {registration.BibNumber} - {registration.RaceName}",
                "payment-reminder" => $"Nhắc nhở thanh toán - {registration.RaceName}",
                "race-day-info" => $"Thông tin ngày thi đấu - {registration.RaceName}",
                _ => "Thông báo từ Ban Tổ Chức"
            };
        }
    }
}