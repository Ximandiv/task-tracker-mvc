using Microsoft.AspNetCore.Mvc;
using Task_Tracker_WebApp.Models.View;

namespace Task_Tracker_WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignInUser(SignInViewModel userSignIn)
        {
            if(!ModelState.IsValid)
                return View(userSignIn);

            ViewData["SignInMessage"] = "Your registration was successful!";

            return View("Login");
        }
    }
}
