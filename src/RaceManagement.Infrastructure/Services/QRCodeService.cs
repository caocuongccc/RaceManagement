using Microsoft.Extensions.Options;
using QRCoder;
using RaceManagement.Core.Interfaces;
using RaceManagement.Core.Models;
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

        public QRCodeService(IOptions<EmailConfiguration> emailConfig)
        {
            _settings = emailConfig.Value.QRCode;
        }

        public async Task<byte[]> GenerateQRCodeAsync(string content, int size = 200)
        {
            return await Task.Run(() =>
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);

                return qrCode.GetGraphic(size / 25); // Convert size to pixels per module
            });
        }

        public async Task<byte[]> GeneratePaymentQRCodeAsync(string transactionReference, decimal amount, string bankInfo)
        {
            var paymentString = await GeneratePaymentStringAsync(transactionReference, amount, bankInfo);
            return await GenerateQRCodeAsync(paymentString, _settings.Size);
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
    }
}
