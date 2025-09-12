using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaceManagement.Core.Interfaces;
using RaceManagement.Core.Models;
using RaceManagement.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Infrastructure.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly EmailTemplateSettings _settings;
        private readonly ILogger<EmailTemplateService> _logger;

        public EmailTemplateService(IOptions<EmailConfiguration> emailConfig, ILogger<EmailTemplateService> logger)
        {
            _settings = emailConfig.Value.Templates;
            _logger = logger;
        }

        public async Task<string> RenderTemplateAsync(string templateName, object model)
        {
            try
            {
                var templatePath = GetTemplatePath(templateName);

                if (!File.Exists(templatePath))
                {
                    _logger.LogWarning("Template file not found: {TemplatePath}", templatePath);
                    return GenerateDefaultTemplate(templateName, model);
                }

                var templateContent = await File.ReadAllTextAsync(templatePath);
                return await RenderTemplateContentAsync(templateContent, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering template {TemplateName}", templateName);
                return GenerateDefaultTemplate(templateName, model);
            }
        }

        public async Task<bool> TemplateExistsAsync(string templateName)
        {
            var templatePath = GetTemplatePath(templateName);
            return await Task.FromResult(File.Exists(templatePath));
        }

        public async Task<IEnumerable<string>> GetAvailableTemplatesAsync()
        {
            try
            {
                var templatesDir = Path.Combine(Directory.GetCurrentDirectory(), _settings.TemplatesPath);

                if (!Directory.Exists(templatesDir))
                {
                    Directory.CreateDirectory(templatesDir);
                    await CreateDefaultTemplatesAsync(templatesDir);
                }

                return await Task.FromResult(
                    Directory.GetFiles(templatesDir, "*.html")
                             .Select(Path.GetFileNameWithoutExtension)
                             .Where(name => !string.IsNullOrEmpty(name))
                             .Cast<string>()
                             .ToList()
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available templates");
                return new List<string>();
            }
        }

        public string GetTemplateSubject(string templateName, object model)
        {
            return templateName.ToLower() switch
            {
                "registration-confirmation" when model is RegistrationDto reg =>
                    $"Xác nhận đăng ký {reg.RaceName} - {reg.FullName}",
                "bib-notification" when model is RegistrationDto reg =>
                    $"Số BIB {reg.BibNumber} - {reg.RaceName}",
                "payment-reminder" when model is RegistrationDto reg =>
                    $"Nhắc nhở thanh toán - {reg.RaceName}",
                "race-day-info" when model is RegistrationDto reg =>
                    $"Thông tin ngày thi đấu - {reg.RaceName}",
                _ => "Thông báo từ Ban Tổ Chức"
            };
        }

        // Helper methods
        private string GetTemplatePath(string templateName)
        {
            var templatesDir = Path.Combine(Directory.GetCurrentDirectory(), _settings.TemplatesPath);
            return Path.Combine(templatesDir, $"{templateName}.html");
        }

        private async Task<string> RenderTemplateContentAsync(string templateContent, object model)
        {
            // Simple template rendering - can be enhanced with Razor Pages or Handlebars
            var result = templateContent;

            if (model is RegistrationDto registration)
            {
                result = result
                    .Replace("{{FullName}}", registration.FullName)
                    .Replace("{{BibName}}", registration.BibName)
                    .Replace("{{Email}}", registration.Email)
                    .Replace("{{Phone}}", registration.Phone ?? "")
                    .Replace("{{RaceName}}", registration.RaceName)
                    .Replace("{{Distance}}", registration.Distance)
                    .Replace("{{BibNumber}}", registration.BibNumber ?? "Chưa có")
                    .Replace("{{TransactionReference}}", registration.TransactionReference)
                    .Replace("{{Price}}", $"{registration.Price:N0}")
                    .Replace("{{ShirtInfo}}", registration.ShirtFullDescription)
                    .Replace("{{CompanyName}}", _settings.CompanyName)
                    .Replace("{{LogoUrl}}", _settings.LogoUrl)
                    .Replace("{{SupportEmail}}", _settings.SupportEmail)
                    .Replace("{{WebsiteUrl}}", _settings.WebsiteUrl)
                    .Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());
            }

            return await Task.FromResult(result);
        }

        private string GenerateDefaultTemplate(string templateName, object model)
        {
            if (model is RegistrationDto registration)
            {
                return templateName.ToLower() switch
                {
                    "registration-confirmation" => GenerateRegistrationConfirmationTemplate(registration),
                    "bib-notification" => GenerateBibNotificationTemplate(registration),
                    "payment-reminder" => GeneratePaymentReminderTemplate(registration),
                    "race-day-info" => GenerateRaceDayInfoTemplate(registration),
                    _ => GenerateGenericTemplate(registration)
                };
            }

            return "<html><body><h1>Email Template</h1><p>No template available.</p></body></html>";
        }

        private async Task CreateDefaultTemplatesAsync(string templatesDir)
        {
            var templates = new Dictionary<string, string>
            {
                ["registration-confirmation"] = GetRegistrationConfirmationTemplate(),
                ["bib-notification"] = GetBibNotificationTemplate(),
                ["payment-reminder"] = GetPaymentReminderTemplate(),
                ["race-day-info"] = GetRaceDayInfoTemplate()
            };

            foreach (var template in templates)
            {
                var filePath = Path.Combine(templatesDir, $"{template.Key}.html");
                await File.WriteAllTextAsync(filePath, template.Value);
            }

            _logger.LogInformation("Created {Count} default email templates", templates.Count);
        }

        private string GenerateRegistrationConfirmationTemplate(RegistrationDto registration)
        {
            return $"""
        <html>
        <head>
            <meta charset="utf-8">
            <title>Xác nhận đăng ký</title>
        </head>
        <body style="font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5;">
            <div style="max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px;">
                <h1 style="color: #2563eb; text-align: center;">Xác nhận đăng ký thành công!</h1>
                <p>Chào <strong>{registration.FullName}</strong>,</p>
                <p>Chúc mừng bạn đã đăng ký thành công <strong>{registration.RaceName}</strong>!</p>
                
                <div style="background: #f8fafc; padding: 15px; border-radius: 5px; margin: 20px 0;">
                    <h3>Thông tin đăng ký:</h3>
                    <ul>
                        <li><strong>Họ tên:</strong> {registration.FullName}</li>
                        <li><strong>Cự ly:</strong> {registration.Distance}</li>
                        <li><strong>Mã tham chiếu:</strong> {registration.TransactionReference}</li>
                        <li><strong>Số tiền:</strong> {registration.Price:N0} VNĐ</li>
                        <li><strong>Thông tin áo:</strong> {registration.ShirtFullDescription}</li>
                    </ul>
                </div>
                
                <div style="background: #fef3c7; padding: 15px; border-radius: 5px; margin: 20px 0;">
                    <h3>Thanh toán:</h3>
                    <p>Vui lòng chuyển khoản với nội dung: <strong>{registration.TransactionReference}</strong></p>
                    <p>Sau khi thanh toán, bạn sẽ nhận được email thông báo số BIB.</p>
                </div>
                
                <p>Cảm ơn bạn đã tham gia!</p>
                <p>Ban Tổ Chức</p>
            </div>
        </body>
        </html>
        """;
        }

        private string GenerateBibNotificationTemplate(RegistrationDto registration)
        {
            return $"""
        <html>
        <head>
            <meta charset="utf-8">
            <title>Thông báo số BIB</title>
        </head>
        <body style="font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5;">
            <div style="max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px;">
                <h1 style="color: #059669; text-align: center;">Số BIB của bạn đã được xác nhận!</h1>
                <p>Chào <strong>{registration.FullName}</strong>,</p>
                <p>Thanh toán của bạn đã được xác nhận thành công!</p>
                
                <div style="background: #ecfdf5; padding: 20px; border-radius: 5px; margin: 20px 0; text-align: center;">
                    <h2 style="color: #059669; margin: 0;">Số BIB của bạn</h2>
                    <div style="font-size: 48px; font-weight: bold; color: #059669; margin: 10px 0;">
                        {registration.BibNumber}
                    </div>
                    <p><strong>{registration.RaceName}</strong></p>
                    <p>Cự ly: <strong>{registration.Distance}</strong></p>
                </div>
                
                <div style="background: #f8fafc; padding: 15px; border-radius: 5px; margin: 20px 0;">
                    <h3>Thông tin quan trọng:</h3>
                    <ul>
                        <li>Vui lòng nhớ số BIB của bạn</li>
                        <li>Đến sớm để nhận BIB và áo đấu</li>
                        <li>Mang theo CMND/CCCD để đối chiếu</li>
                    </ul>
                </div>
                
                <p>Chúc bạn thi đấu thành công!</p>
                <p>Ban Tổ Chức</p>
            </div>
        </body>
        </html>
        """;
        }

        // Template definitions for file creation
        private string GetRegistrationConfirmationTemplate()
        {
            return """
        <html>
        <head>
            <meta charset="utf-8">
            <title>Xác nhận đăng ký</title>
        </head>
        <body style="font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5;">
            <div style="max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px;">
                <h1 style="color: #2563eb; text-align: center;">Xác nhận đăng ký thành công!</h1>
                <p>Chào <strong>{{FullName}}</strong>,</p>
                <p>Chúc mừng bạn đã đăng ký thành công <strong>{{RaceName}}</strong>!</p>
                
                <div style="background: #f8fafc; padding: 15px; border-radius: 5px; margin: 20px 0;">
                    <h3>Thông tin đăng ký:</h3>
                    <ul>
                        <li><strong>Họ tên:</strong> {{FullName}}</li>
                        <li><strong>Cự ly:</strong> {{Distance}}</li>
                        <li><strong>Mã tham chiếu:</strong> {{TransactionReference}}</li>
                        <li><strong>Số tiền:</strong> {{Price}} VNĐ</li>
                        <li><strong>Thông tin áo:</strong> {{ShirtInfo}}</li>
                    </ul>
                </div>
                
                <div style="background: #fef3c7; padding: 15px; border-radius: 5px; margin: 20px 0;">
                    <h3>Thanh toán:</h3>
                    <p>Vui lòng chuyển khoản với nội dung: <strong>{{TransactionReference}}</strong></p>
                    <p>Sau khi thanh toán, bạn sẽ nhận được email thông báo số BIB.</p>
                </div>
                
                <p>Cảm ơn bạn đã tham gia!</p>
                <p>{{CompanyName}}</p>
            </div>
        </body>
        </html>
        """;
        }

        private string GetBibNotificationTemplate()
        {
            return """
        <html>
        <head>
            <meta charset="utf-8">
            <title>Thông báo số BIB</title>
        </head>
        <body style="font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5;">
            <div style="max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px;">
                <h1 style="color: #059669; text-align: center;">Số BIB của bạn đã được xác nhận!</h1>
                <p>Chào <strong>{{FullName}}</strong>,</p>
                <p>Thanh toán của bạn đã được xác nhận thành công!</p>
                
                <div style="background: #ecfdf5; padding: 20px; border-radius: 5px; margin: 20px 0; text-align: center;">
                    <h2 style="color: #059669; margin: 0;">Số BIB của bạn</h2>
                    <div style="font-size: 48px; font-weight: bold; color: #059669; margin: 10px 0;">
                        {{BibNumber}}
                    </div>
                    <p><strong>{{RaceName}}</strong></p>
                    <p>Cự ly: <strong>{{Distance}}</strong></p>
                </div>
                
                <p>Chúc bạn thi đấu thành công!</p>
                <p>{{CompanyName}}</p>
            </div>
        </body>
        </html>
        """;
        }

        private string GetPaymentReminderTemplate()
        {
            return """
        <html>
        <head>
            <meta charset="utf-8">
            <title>Nhắc nhở thanh toán</title>
        </head>
        <body style="font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5;">
            <div style="max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px;">
                <h1 style="color: #f59e0b; text-align: center;">Nhắc nhở thanh toán</h1>
                <p>Chào <strong>{{FullName}}</strong>,</p>
                <p>Bạn đã đăng ký thành công <strong>{{RaceName}}</strong> nhưng chưa hoàn tất thanh toán.</p>
                
                <div style="background: #fef3c7; padding: 15px; border-radius: 5px; margin: 20px 0;">
                    <h3>Thông tin thanh toán:</h3>
                    <ul>
                        <li><strong>Mã tham chiếu:</strong> {{TransactionReference}}</li>
                        <li><strong>Số tiền:</strong> {{Price}} VNĐ</li>
                    </ul>
                </div>
                
                <p>Vui lòng hoàn tất thanh toán để nhận số BIB.</p>
                <p>{{CompanyName}}</p>
            </div>
        </body>
        </html>
        """;
        }

        private string GetRaceDayInfoTemplate()
        {
            return """
        <html>
        <head>
            <meta charset="utf-8">
            <title>Thông tin ngày thi đấu</title>
        </head>
        <body style="font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5;">
            <div style="max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px;">
                <h1 style="color: #7c3aed; text-align: center;">Thông tin ngày thi đấu</h1>
                <p>Chào <strong>{{FullName}}</strong>,</p>
                <p><strong>{{RaceName}}</strong> sẽ diễn ra sớm. Dưới đây là thông tin quan trọng:</p>
                
                <div style="background: #f3e8ff; padding: 15px; border-radius: 5px; margin: 20px 0;">
                    <h3>Thông tin của bạn:</h3>
                    <ul>
                        <li><strong>Số BIB:</strong> {{BibNumber}}</li>
                        <li><strong>Cự ly:</strong> {{Distance}}</li>
                    </ul>
                </div>
                
                <p>Chúc bạn thi đấu thành công!</p>
                <p>{{CompanyName}}</p>
            </div>
        </body>
        </html>
        """;
        }

        private string GeneratePaymentReminderTemplate(RegistrationDto registration)
        {
            return $"""
        <html>
        <head>
            <meta charset="utf-8">
            <title>Nhắc nhở thanh toán</title>
        </head>
        <body style="font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5;">
            <div style="max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px;">
                <h1 style="color: #f59e0b; text-align: center;">Nhắc nhở thanh toán</h1>
                <p>Chào <strong>{registration.FullName}</strong>,</p>
                <p>Bạn đã đăng ký thành công <strong>{registration.RaceName}</strong> nhưng chưa hoàn tất thanh toán.</p>
                
                <div style="background: #fef3c7; padding: 15px; border-radius: 5px; margin: 20px 0;">
                    <h3>Thông tin thanh toán:</h3>
                    <ul>
                        <li><strong>Mã tham chiếu:</strong> {registration.TransactionReference}</li>
                        <li><strong>Số tiền:</strong> {registration.Price:N0} VNĐ</li>
                    </ul>
                </div>
                
                <p>Vui lòng hoàn tất thanh toán để nhận số BIB.</p>
                <p>Ban Tổ Chức</p>
            </div>
        </body>
        </html>
        """;
        }

        private string GenerateRaceDayInfoTemplate(RegistrationDto registration)
        {
            return $"""
        <html>
        <head>
            <meta charset="utf-8">
            <title>Thông tin ngày thi đấu</title>
        </head>
        <body style="font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5;">
            <div style="max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px;">
                <h1 style="color: #7c3aed; text-align: center;">Thông tin ngày thi đấu</h1>
                <p>Chào <strong>{registration.FullName}</strong>,</p>
                <p><strong>{registration.RaceName}</strong> sẽ diễn ra sớm. Dưới đây là thông tin quan trọng:</p>
                
                <div style="background: #f3e8ff; padding: 15px; border-radius: 5px; margin: 20px 0;">
                    <h3>Thông tin của bạn:</h3>
                    <ul>
                        <li><strong>Số BIB:</strong> {registration.BibNumber ?? "Chưa có"}</li>
                        <li><strong>Cự ly:</strong> {registration.Distance}</li>
                    </ul>
                </div>
                
                <p>Chúc bạn thi đấu thành công!</p>
                <p>Ban Tổ Chức</p>
            </div>
        </body>
        </html>
        """;
        }

        private string GenerateGenericTemplate(RegistrationDto registration)
        {
            return $"""
        <html>
        <head>
            <meta charset="utf-8">
            <title>Thông báo</title>
        </head>
        <body style="font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5;">
            <div style="max-width: 600px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px;">
                <h1>Thông báo từ Ban Tổ Chức</h1>
                <p>Chào <strong>{registration.FullName}</strong>,</p>
                <p>Đây là thông báo liên quan đến việc tham gia <strong>{registration.RaceName}</strong>.</p>
                <p>Cảm ơn bạn!</p>
                <p>Ban Tổ Chức</p>
            </div>
        </body>
        </html>
        """;
        }
    }
}
