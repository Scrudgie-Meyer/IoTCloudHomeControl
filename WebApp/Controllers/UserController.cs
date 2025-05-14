using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using WebApp.Models;




namespace WebApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiBaseUrl = "https://localhost:5001/api/scenario";

        public UserController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        // Створити форму створення івенту
        [HttpGet]
        public IActionResult EventCreator()
        {
            return View(new EventCreatorModel());
        }

        // Надіслати POST до API
        [HttpPost]
        public async Task<IActionResult> CreateSingle(EventCreatorModel dto)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/schedule", content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("CreateSingle");
            }

            ModelState.AddModelError("", "Помилка створення івенту");
            return View(dto);
        }
    }
}
