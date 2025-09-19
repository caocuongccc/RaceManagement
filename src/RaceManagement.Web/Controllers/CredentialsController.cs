using Microsoft.AspNetCore.Mvc;
using RaceManagement.Shared.DTOs;
using RaceManagement.Web.Models;
using RaceManagement.Web.Services.ApiClient;

namespace RaceManagement.Web.Controllers
{
    public class CredentialsController : Controller
    {
        private readonly ICredentialApiClient _client;

        public CredentialsController(ICredentialApiClient client)
        {
            _client = client;
        }

        public async Task<IActionResult> Index()
        {
            var creds = await _client.GetAllAsync();
            return View(creds);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateCredentialDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCredentialDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            
            var ok = await _client.CreateAsync(dto);
            if (ok) return RedirectToAction("Index");

            ModelState.AddModelError("", "Lỗi khi tạo Credential");
            return View(dto);
        }
    }
}
