using OfficeOpenXml.Style;
using OfficeOpenXml;
using RaceManagement.Shared.DTOs;

namespace RaceManagement.Web.Services.Export
{
    public class ExcelExportService
    {
        public byte[] ExportRegistrations(IEnumerable<RegistrationDto> registrations)
        {
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage.License.SetNonCommercialPersonal("Cuong.LeCao");
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Registrations");

            // Header
            var headers = new[]
            {
                "ID", "Race", "Distance", "Full Name", "Bib Name", "Email", "Phone",
                "DOB", "Gender", "Shirt", "Registration Time", "Payment Status",
                "Bib Number", "Bib Sent At", "Transaction Ref"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[1, i + 1].Value = headers[i];
                ws.Cells[1, i + 1].Style.Font.Bold = true;
                ws.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Data
            var row = 2;
            foreach (var r in registrations)
            {
                ws.Cells[row, 1].Value = r.Id;
                ws.Cells[row, 2].Value = r.RaceName;
                ws.Cells[row, 3].Value = r.Distance;
                ws.Cells[row, 4].Value = r.FullName;
                ws.Cells[row, 5].Value = r.BibName;
                ws.Cells[row, 6].Value = r.Email;
                ws.Cells[row, 7].Value = r.Phone;
                ws.Cells[row, 8].Value = r.DateOfBirth?.ToString("yyyy-MM-dd") ?? r.RawBirthInput;
                ws.Cells[row, 9].Value = r.Gender;
                ws.Cells[row, 10].Value = r.ShirtFullDescription;
                ws.Cells[row, 11].Value = r.RegistrationTime.ToString("yyyy-MM-dd HH:mm");
                ws.Cells[row, 12].Value = r.PaymentStatus;
                ws.Cells[row, 13].Value = r.BibNumber;
                ws.Cells[row, 14].Value = r.BibSentAt?.ToString("yyyy-MM-dd HH:mm");
                ws.Cells[row, 15].Value = r.TransactionReference;
                row++;
            }

            ws.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }
    }
}
