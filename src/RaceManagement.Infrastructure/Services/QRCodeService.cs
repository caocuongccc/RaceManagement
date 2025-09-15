using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QRCoder;
using RaceManagement.Core.Entities;
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
    public class QRCodeService : IQRCodeService
    {
        private readonly QRCodeSettings _settings;
        private readonly ILogger<QRCodeService> _logger;

        public QRCodeService(IOptions<EmailConfiguration> emailConfig, ILogger<QRCodeService> logger)
        {
            _settings = emailConfig.Value.QRCode;
            _logger = logger;
        }

        public async Task<byte[]> GenerateQRCodeAsync(string content, int size = 200)
        {
            try
            {
                return await Task.Run(() =>
                {
                    using var qrGenerator = new QRCodeGenerator();
                    using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
                    using var qrCode = new PngByteQRCode(qrCodeData);

                    return qrCode.GetGraphic(size / 25); // Convert size to pixels per module
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate QR code for content: {Content}", content);
                throw;
            }
        }

        public async Task<byte[]> GeneratePaymentQRCodeAsync(string transactionReference, decimal amount, string bankInfo)
        {
            try
            {
                var paymentString = await GeneratePaymentStringAsync(transactionReference, amount, bankInfo);
                return await GenerateQRCodeAsync(paymentString, _settings.Size);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate payment QR code for registration {RegistrationId}", transactionReference);
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
        public async Task<string> GeneratePaymentStringAsync(string transactionReference, decimal amount, string bankInfo)
        {
            // Vietnamese QR payment format
            var paymentInfo = $"""
            Ngân hàng: {_settings.BankName}
            Số tài khoản: {_settings.BankAccount}
            Chủ tài khoản: {_settings.BankAccountName}
            Số tiền: {amount:N0} VND
            Nội dung: {transactionReference}
            
            Vui lòng chuyển khoản đúng số tiền và nội dung để được xác nhận tự động.
            """;

            return await Task.FromResult(paymentInfo);
        }

        public async Task<byte[]> GenerateBibQRCodeAsync(string bibNumber, string raceName)
        {
            var content = $"BIB: {bibNumber}\nRace: {raceName}";
            return await GenerateQRCodeAsync(content, _settings.Size);
        }

        public async Task<byte[]> GenerateQRCodeWithLogoAsync(string content, byte[]? logoBytes = null)
        {
            try
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

                if (logoBytes != null)
                {
                    // Add logo to QR code if provided
                    using var qrCode = new PngByteQRCode(qrCodeData);
                    return await Task.FromResult(qrCode.GetGraphic(20));
                }
                else
                {
                    using var qrCode = new PngByteQRCode(qrCodeData);
                    return await Task.FromResult(qrCode.GetGraphic(20));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate QR code with logo");
                throw;
            }
        }
    }
}
