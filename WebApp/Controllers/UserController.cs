using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using WebApp.Models;




namespace WebApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public UserController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public IActionResult ManagePanel() => View();
        public IActionResult Instruction() => View();

        public async Task<IActionResult> EventCreator()
        {
            List<UserDeviceDto>? devices = new();

            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var apiUrl = _configuration["ApiBaseUrl"] + $"/api/Device/user/{userId}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    devices = await response.Content.ReadFromJsonAsync<List<UserDeviceDto>>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Фільтруємо Mobile та IoT, позначаємо IoT як неактивні
            var deviceOptions = devices
                .Where(d => d.Type == "Mobile" || d.Type == "IoT")
                .Select(d => new DeviceOptionModel
                {
                    Id = d.Id,
                    Name = d.Type == "IoT" ? $"{d.Name} (IoT – not supported yet)" : d.Name,
                    ParentDeviceId = d.ParentDeviceId
                })
                .ToList();

            ViewBag.DeviceOptions = deviceOptions;
            return View();
        }



        // Надіслати POST до API
        [HttpPost]
        public async Task<IActionResult> CreateSingle(EventCreatorModel dto)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var apiUrl = $"{_configuration["ApiBaseUrl"]}/api/Scenario/schedule";
            var response = await _httpClient.PostAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("ManagePanel");
            }

            ModelState.AddModelError("", "Помилка створення івенту");
            return View(dto);
        }
    }
    

    public class UserDeviceDto
    {
        public int Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public required string Type { get; set; } = string.Empty;
        public int? ParentDeviceId { get; set; }
    }

}
