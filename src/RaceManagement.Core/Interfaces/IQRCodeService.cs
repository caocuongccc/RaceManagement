using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Core.Interfaces
{
    public interface IQRCodeService
    {
        Task<byte[]> GenerateQRCodeAsync(string content, int size = 200);
        Task<byte[]> GeneratePaymentQRCodeAsync(string transactionReference, decimal amount, string bankInfo);
        Task<string> GeneratePaymentStringAsync(string transactionReference, decimal amount, string bankInfo);
        Task<byte[]> GenerateBibQRCodeAsync(string bibNumber, string raceName);
    }
}
