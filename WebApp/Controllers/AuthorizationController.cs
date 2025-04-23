using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class AuthorizationController : Controller
    {
        public IActionResult Register() => View();
        public IActionResult Login() => View();

        // POST: /Register
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "Registration successful!";
                return RedirectToAction("Register");
            }

            return View(model);
        }
    }
}
