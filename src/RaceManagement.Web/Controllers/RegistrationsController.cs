using Microsoft.AspNetCore.Mvc;
using RaceManagement.Application.Services;
using RaceManagement.Web.Services.ApiClient;
using RaceManagement.Web.Services.Export;
using System.Text;

namespace RaceManagement.Web.Controllers
{
    public class RegistrationsController : Controller
    {
        private readonly IRegistrationApiClient _regClient;
        private readonly IRegistrationService _registrationService;
        private readonly ExcelExportService _excelExportService;

        public RegistrationsController(IRegistrationApiClient regClient, IRegistrationService registrationService, ExcelExportService excelExportService)
        {
            _regClient = regClient;
            _registrationService = registrationService;
            _excelExportService = excelExportService;
        }

        public async Task<IActionResult> Index(int raceId)
        {
            var regs = await _regClient.GetByRaceAsync(raceId);
            ViewBag.RaceId = raceId;
            return View(regs);
        }

        [HttpPost]
        public async Task<IActionResult> MarkPaid(int id, int raceId)
        {
            await _regClient.MarkPaidAsync(id);
            return RedirectToAction(nameof(Index), new { raceId });
        }
        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportExcel()
        {
            var registrations = await _registrationService.GetAllAsync();
            var bytes = _excelExportService.ExportRegistrations(registrations);

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"registrations_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
        [HttpGet("export")]
        public async Task<IActionResult> ExportRegistrations()
        {
            var registrations = await _registrationService.GetAllAsync(); // hoặc từ DbContext
            var lines = new List<string>
    {
        "FullName,BibName,Email,Phone,Distance,Price,Shirt,BibNumber,PaymentStatus,Age,RegistrationTime"
    };

            foreach (var r in registrations)
            {
                lines.Add(string.Join(",",
                    r.FullName,
                    r.BibName,
                    r.Email,
                    r.Phone,
                    r.Distance,
                    r.Price,
                    r.ShirtFullDescription,
                    r.BibNumber,
                    r.PaymentStatus,
                    r.Age,
                    r.RegistrationTime.ToString("yyyy-MM-dd HH:mm")
                ));
            }

            var csv = string.Join("\n", lines);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "registrations.csv");
        }

    }
}
