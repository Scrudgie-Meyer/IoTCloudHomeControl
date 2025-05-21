using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuthorizationController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient(nameof(AuthorizationController));
            _configuration = configuration;
        }

        public IActionResult Register() => View();

        public IActionResult Login() => View();

        public IActionResult RecoverPassword() => View();



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var loginRequest = new
            {
                Email = model.Email,
                Password = model.Password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            var apiUrl = _configuration["ApiBaseUrl"] + "/api/User/authenticate";

            var response = await _httpClient.PostAsync(apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                string message = response.StatusCode switch
                {
                    HttpStatusCode.Forbidden => "Email not confirmed.",
                    HttpStatusCode.Unauthorized => "Invalid credentials.",
                    _ => "Login failed."
                };

                return RedirectToAction("Login", new { error = message });
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserDto>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new InvalidOperationException("Failed to deserialize the user from the response.");

            // Створення claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddHours(1)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            return RedirectToAction("ManagePanel", "User");
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var hasher = new PasswordHasher<RegisterViewModel>();
            string hashedPassword = hasher.HashPassword(model, model.Password);

            var user = new
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = hashedPassword
            };

            var content = new StringContent(
                JsonSerializer.Serialize(user),
                Encoding.UTF8,
                "application/json"
            );

            var apiUrl = _configuration["ApiBaseUrl"] + "/api/user";
            var response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Registration successful!";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, "Registration failed.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            var apiUrl = _configuration["ApiBaseUrl"] + $"api/user/{token}/update-status";

            var update = new
            {
                IsEmailConfirmed = true,
                Hidden = false
            };

            var json = JsonSerializer.Serialize(update);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(apiUrl, content);

            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                return NotFound("Invalid confirmation token.");
            }

            return Ok("Email confirmed successfully.");
        }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public bool IsAdmin { get; set; }
    }

}
