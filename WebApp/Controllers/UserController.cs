﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WebApp.Models;




namespace WebApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public UserController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient(nameof(AuthorizationController));
            _configuration = configuration;
        }

        public IActionResult Instruction() => View();

        public async Task<IActionResult> ManagePanel()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var apiUrl = _configuration["ApiBaseUrl"] + $"/api/Scenario/user/{userId}/events";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
                return View("Error");

            var json = await response.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<ScheduledEventViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(events);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var apiUrl = _configuration["ApiBaseUrl"] + $"/api/scenario/event/{id}";
            var response = await _httpClient.DeleteAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to delete event.";
                return RedirectToAction("ManagePanel");
            }

            return RedirectToAction("ManagePanel");
        }

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

        [HttpPost]
        public async Task<IActionResult> CreateSingle(EventCreatorModel dto)
        {
            var json = JsonSerializer.Serialize(dto);
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

        public async Task<IActionResult> ToggleEventStatus(int id)
        {
            var apiUrl = $"{_configuration["ApiBaseUrl"]}/api/scenario/{id}/toggle";

            var response = await _httpClient.PutAsync(apiUrl, null);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return Content(json, "application/json");
            }

            return StatusCode((int)response.StatusCode, "Failed to toggle status.");
        }

    }

    public class ToggleEventStatusDto
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
    }


    public class UserDeviceDto
    {
        public int Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public required string Type { get; set; } = string.Empty;
        public int? ParentDeviceId { get; set; }
    }

}
