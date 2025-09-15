using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QRCoder;
using RaceManagement.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using RaceManagement.Core.Interfaces;


namespace RaceManagement.Infrastructure.Repositories
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<EmailResult> SendEmailAsync(EmailRequest request)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var smtpHost = smtpSettings["Host"];
                var smtpPort = int.Parse(smtpSettings["Port"] ?? "587");
                var smtpUser = smtpSettings["Username"];
                var smtpPass = smtpSettings["Password"];

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true")
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(smtpUser, request.To ?? "Race Management"),
                    Subject = request.Subject,
                    Body = request.HtmlBody ?? request.HtmlBody,
                    IsBodyHtml = !string.IsNullOrEmpty(request.HtmlBody)
                };

                message.To.Add(request.To);

                //if (request.Cc?.Any() == true)
                //{
                //    foreach (var cc in request.Cc)
                //        message.CC.Add(cc);
                //}

                //if (request.Bcc?.Any() == true)
                //{
                //    foreach (var bcc in request.Bcc)
                //        message.Bcc.Add(bcc);
                //}

                // Add attachments
                if (request.Attachments?.Any() == true)
                {
                    foreach (var attachment in request.Attachments)
                    {
                        var stream = new MemoryStream(attachment.Content);
                        var mailAttachment = new Attachment(stream, attachment.FileName, attachment.ContentType);
                        message.Attachments.Add(mailAttachment);
                    }
                }

                await client.SendMailAsync(message);

                _logger.LogInformation("Email sent successfully to {To}", request.To);

                return new EmailResult
                {
                    IsSuccess = true,
                    MessageId = Guid.NewGuid().ToString(),
                    SentAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", request.To);
                return new EmailResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    SentAt = DateTime.UtcNow
                };
            }
        }

        public async Task<byte[]> GenerateQRCodeAsync(string content)
        {
            try
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);

                return await Task.FromResult(qrCode.GetGraphic(20));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate QR code for content: {Content}", content);
                throw;
            }
        }

        public async Task<byte[]> GeneratePaymentQRCodeAsync(RegistrationDto registration)
        {
            try
            {
                // Generate payment QR code content
                var qrContent = $"BANK_TRANSFER|{registration.Id}|{registration.Email}|{registration.Price}";
                return await GenerateQRCodeAsync(qrContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate payment QR code for registration {RegistrationId}", registration.Id);
                throw;
            }
        }
    }
}
