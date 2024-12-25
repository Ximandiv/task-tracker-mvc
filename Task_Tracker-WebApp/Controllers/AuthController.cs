using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Tracker_WebApp.Auth;
using Task_Tracker_WebApp.Database;
using Task_Tracker_WebApp.Database.Entities;
using Task_Tracker_WebApp.Models.View;

namespace Task_Tracker_WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly TaskContext _taskContext;
        private readonly JWTGenerator _jwtGenerator;

        public AuthController(
            ILogger<AuthController> logger,
            TaskContext taskContext,
            JWTGenerator jwtGenerator)
        {
            _logger = logger;
            _taskContext = taskContext;
            _jwtGenerator = jwtGenerator;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogInUser(LogInViewModel userLogIn)
        {
            User? foundUser = await _taskContext.Users
                                            .Where(u => u.Email == userLogIn.Email)
                                            .FirstOrDefaultAsync();

            if(foundUser == null)
            {
                ViewData["ErrorLogin"] = "User Account Credentials Invalid";
                return View("Login");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(userLogIn.Password, foundUser.Password);

            if (!isPasswordValid)
            {
                ViewData["ErrorLogin"] = "User Account Credentials Invalid";
                return View("Login");
            }

            var token = _jwtGenerator.GenerateToken(foundUser);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(12)
            };
            Response.Cookies.Append("JWToken", token, cookieOptions);

            return RedirectToAction("Dashboard", "TaskHome");
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignInUser(SignInViewModel userSignIn)
        {
            if(!ModelState.IsValid)
                return View(userSignIn);

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userSignIn.Password);

            using var transaction = await _taskContext.Database.BeginTransactionAsync();

            try
            {
                User newUser = new()
                {
                    Username = userSignIn.Username!,
                    Email = userSignIn.Email!,
                    Password = hashedPassword!,
                };

                _taskContext.Users.Add(newUser);
                await _taskContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"An error occurred during User Registration with email {userSignIn.Email} because: {ex.Message}");
            }

            ViewData["SignInMessage"] = "Your registration was successful!";

            return View("Login");
        }
    }
}
