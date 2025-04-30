using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public IActionResult SetPreferences(string lang = "uk-UA", string timezone = "Europe/Kyiv")
        {
            HttpContext.Session.SetString("Language", lang);
            HttpContext.Session.SetString("TimeZone", timezone);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            string apiUrl = "http://localhost:5000/api/user/1";
            string userName;

            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                userName = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Failed to fetch user: {Message}", ex.Message);
                userName = "Error fetching user";
            }

            var lang = HttpContext.Session.GetString("Language") ?? "не задано";
            var tz = HttpContext.Session.GetString("TimeZone") ?? "не задано";

            ViewBag.Language = lang;
            ViewBag.TimeZone = tz;

            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
