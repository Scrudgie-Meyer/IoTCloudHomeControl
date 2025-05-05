using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;



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
    }
}
