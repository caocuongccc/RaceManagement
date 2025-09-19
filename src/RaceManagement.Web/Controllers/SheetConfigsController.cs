using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RaceManagement.Shared.DTOs;
using RaceManagement.Web.Services.ApiClient;

namespace RaceManagement.Web.Controllers
{
    public class SheetConfigsController : Controller
    {
        private readonly ISheetConfigApiClient _sheetClient;
        private readonly ICredentialApiClient _credClient;

        public SheetConfigsController(ISheetConfigApiClient sheetClient, ICredentialApiClient credClient)
        {
            _sheetClient = sheetClient;
            _credClient = credClient;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _sheetClient.GetAllAsync();
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var creds = await _credClient.GetAllAsync();
            ViewBag.Credentials = new SelectList(creds, "Id", "Name");
            return View(new CreateSheetConfigDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSheetConfigDto dto)
        {
            if (!ModelState.IsValid)
            {
                var creds = await _credClient.GetAllAsync();
                ViewBag.Credentials = new SelectList(creds, "Id", "Name");
                return View(dto);
            }

            var ok = await _sheetClient.CreateAsync(dto);
            if (ok) return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Lỗi khi tạo SheetConfig");
            var creds2 = await _credClient.GetAllAsync();
            ViewBag.Credentials = new SelectList(creds2, "Id", "Name");
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var config = await _sheetClient.GetByIdAsync(id);
            if (config == null) return NotFound();

            var dto = new UpdateSheetConfigDto
            {
                Name = config.Name,
                SheetName = config.SheetName,
                Description = config.Description,
                HeaderRowIndex = config.HeaderRowIndex,
                DataStartRowIndex = config.DataStartRowIndex,
                IsActive = config.IsActive
            };

            var creds = await _credClient.GetAllAsync();
            ViewBag.CredentialName = config.CredentialName;
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateSheetConfigDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var ok = await _sheetClient.UpdateAsync(id, dto);
            if (ok) return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Lỗi khi cập nhật SheetConfig");
            return View(dto);
        }
    }
}
