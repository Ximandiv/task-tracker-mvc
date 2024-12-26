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
        private readonly TokenGenerator _tokenGenerator;
        private readonly CookieOptions _accessTokenOptions;
        private readonly CookieOptions _rememberMeOptions;

        private readonly int _accessTokenDuration = 1;
        private readonly int _rememberMeDuration = 3;

        public AuthController(
            ILogger<AuthController> logger,
            TaskContext taskContext,
            TokenGenerator tokenGenerator)
        {
            _logger = logger;
            _taskContext = taskContext;
            _tokenGenerator = tokenGenerator;

            _accessTokenOptions = new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(_accessTokenDuration)
            };
            _rememberMeOptions = new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMonths(_rememberMeDuration)
            };
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if(Request.Cookies.TryGetValue("RememberMe", out var rememberMeToken))
            {
                var tokenRecord = await _taskContext.RememberMeTokens
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Token == rememberMeToken 
                                        && r.Expiration > DateTime.UtcNow);

                if(tokenRecord != null
                    && tokenRecord.User != null)
                {
                    var jwtToken = _tokenGenerator.GenerateJWT(tokenRecord.User);

                    Response.Cookies.Append("JWToken", jwtToken, _accessTokenOptions);

                    return RedirectToAction("Dashboard", "TaskHome");
                }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LogInViewModel userLogIn)
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

            var token = _tokenGenerator.GenerateJWT(foundUser);

            bool wasRememberMeSaved = false;
            if (userLogIn.RememberMe)
            {
                var rememberMe = _tokenGenerator.GenerateRememberMe(foundUser.Id);

                using (var transaction = await _taskContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        await _taskContext.RememberMeTokens.AddAsync(rememberMe);
                        await _taskContext.SaveChangesAsync();

                        await transaction.CommitAsync();
                        wasRememberMeSaved = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($@"An error occurred during 
                                            RememberMeToken creation with message {ex.Message}");
                        await transaction.RollbackAsync();
                        wasRememberMeSaved = false;
                    }
                }

                if(wasRememberMeSaved)
                    Response.Cookies.Append("RememberMe", rememberMe.Token, _rememberMeOptions);
            }

            Response.Cookies.Append("JWToken", token, _accessTokenOptions);

            return RedirectToAction("Dashboard", "TaskHome");
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInViewModel userSignIn)
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
                _logger.LogError($@"
                                An error occurred during User creation
                                with email {userSignIn.Email} with message: {ex.Message}");
            }

            ViewData["SignInMessage"] = "Your registration was successful!";

            return View("Login");
        }
    }
}
