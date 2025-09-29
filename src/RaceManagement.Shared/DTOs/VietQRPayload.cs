using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceManagement.Shared.DTOs
{
    public class VietQRPayload
    {

        public string AcqId { get; set; } = string.Empty;         // Bank BIN
        public string AccountNumber { get; set; } = string.Empty; // Số tài khoản
        public string AccountName { get; set; } = string.Empty;   // Tên chủ tài khoản
        public decimal Amount { get; set; }                      // Số tiền
        public string AddInfo { get; set; } = string.Empty;      // Nội dung chuyển khoản
        public string Template { get; set; } = "compact";


        public static class BankBinProvider
        {
            public static readonly Dictionary<string, string> BankBins = new(StringComparer.OrdinalIgnoreCase)
            {
                { "Vietcombank", "970436" },
                { "Techcombank", "970407" },
                { "MB Bank", "970422" },
                { "BIDV", "970418" },
                { "Agribank", "970405" },
                { "ACB", "970416" },
                { "Sacombank", "970403" },
                { "VPBank", "970432" },
                { "VietinBank", "970415" },
                { "SeABank", "970440" },
                // thêm ngân hàng khác ở đây
            };

            public static string GetBankBin(string? bankName)
            {
                if (string.IsNullOrWhiteSpace(bankName))
                    throw new ArgumentException("Bank name is required");

                return BankBins.TryGetValue(bankName, out var bin)
                    ? bin
                    : throw new KeyNotFoundException($"Bank BIN not found for {bankName}");
            }
        }

    }

}
