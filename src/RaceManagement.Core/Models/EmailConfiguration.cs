using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Core.Models
{
    public class EmailConfiguration
    {
        public SmtpSettings Smtp { get; set; } = new();
        public EmailTemplateSettings Templates { get; set; } = new();
        public EmailJobSettings Jobs { get; set; } = new();
        public QRCodeSettings QRCode { get; set; } = new();
    }

    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public int Timeout { get; set; } = 30000; // 30 seconds
        public int MaxRetries { get; set; } = 3;
    }

    public class EmailTemplateSettings
    {
        public string TemplatesPath { get; set; } = "EmailTemplates";
        public string LogoUrl { get; set; } = string.Empty;
        public string CompanyName { get; set; } = "Race Management System";
        public string SupportEmail { get; set; } = string.Empty;
        public string WebsiteUrl { get; set; } = string.Empty;
    }

    public class EmailJobSettings
    {
        public int BatchSize { get; set; } = 10;
        public int DelayBetweenBatches { get; set; } = 5000; // 5 seconds
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryDelayMinutes { get; set; } = 5;
        public bool EnableRateLimiting { get; set; } = true;
    }

    public class QRCodeSettings
    {
        public int Size { get; set; } = 200;
        public int Margin { get; set; } = 4;
        public string Format { get; set; } = "PNG";
        public string BankName { get; set; } = "Ngân hàng ABC";
        public string BankAccount { get; set; } = "1234567890";
        public string BankAccountName { get; set; } = "Ban Tổ Chức";
    }
}
