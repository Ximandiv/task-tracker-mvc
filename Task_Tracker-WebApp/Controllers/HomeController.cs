using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task_Tracker_WebApp.Models.ViewModel;

namespace Task_Tracker_WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewData["AuthError"] = TempData["AuthError"] ?? null;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult AuthorizedPrivacy()
        {
            ViewData["Username"] = User.FindFirstValue("username");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            string requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            _logger.LogWarning($"Request with id {requestId} had issues");

            return View(new ErrorViewModel { RequestId = requestId });
        }
    }
}
