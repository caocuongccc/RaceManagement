using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RaceManagement.Shared.DTOs;
using RaceManagement.Web.Services.ApiClient;

namespace RaceManagement.Web.Controllers
{
    public class RacesController : Controller
    {
        private readonly IRaceApiClient _raceClient;
        private readonly ISheetConfigApiClient _sheetClient;

        public RacesController(IRaceApiClient raceClient, ISheetConfigApiClient sheetClient)
        {
            _raceClient = raceClient;
            _sheetClient = sheetClient;
        }

        public async Task<IActionResult> Index()
        {
            var races = await _raceClient.GetAllAsync();
            return View(races);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var sheets = await _sheetClient.GetAllAsync();
            ViewBag.SheetConfigs = new SelectList(sheets, "Id", "DisplayName");
            return View(new CreateRaceDto
            {
                Distances = new List<CreateRaceDistanceDto> { new CreateRaceDistanceDto() },
                ShirtTypes = new List<CreateRaceShirtTypeDto> { new CreateRaceShirtTypeDto() }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRaceDto dto)
        {
            if (!ModelState.IsValid)
            {
                var sheets = await _sheetClient.GetAllAsync();
                ViewBag.SheetConfigs = new SelectList(sheets, "Id", "DisplayName");
                return View(dto);
            }

            var ok = await _raceClient.CreateAsync(dto);
            if (ok) return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Lỗi khi tạo Race");
            var sheets2 = await _sheetClient.GetAllAsync();
            ViewBag.SheetConfigs = new SelectList(sheets2, "Id", "DisplayName");
            return View(dto);
        }
    }
}
