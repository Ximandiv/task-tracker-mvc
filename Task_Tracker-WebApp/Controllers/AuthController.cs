using Microsoft.AspNetCore.Mvc;
using Task_Tracker_WebApp.Models.ViewModel;
using Task_Tracker_WebApp.Use_Cases.Auth;
using Task_Tracker_WebApp.Use_Cases.Auth.Models;

namespace Task_Tracker_WebApp.Controllers
{
    public class AuthController
        (CredentialsHandler authService,
        TokenHandler tokenHandler,
        CookieAccessOptions cookieOpts,
        ILogger<AuthController> logger) : Controller
    {
        private readonly CredentialsHandler _authService = authService;
        private readonly TokenHandler _tokenHandler = tokenHandler;
        private readonly CookieAccessOptions _cookieOpts = cookieOpts;
        private readonly ILogger<AuthController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            try
            {
                string jWT = await _tokenHandler.GetRememberToken(Request);

                if (jWT != string.Empty)
                {
                    Response.Cookies.Append("JWToken", jWT, _cookieOpts.GetAccessOptions());

                    return RedirectToAction("Dashboard", "TaskHome");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred in endpoint GET .../Auth/Login");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LogInViewModel viewModel)
        {
            AuthTokens? tokenModel = null;
            try
            {
                tokenModel = await _authService.Login(viewModel.User!);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred in endpoint POST .../Auth/Login");
            }

            if(tokenModel is null)
            {
                ViewData["ErrorLogin"] = "Invalid credentials";
                return View();
            }
            
            if(tokenModel.RememberMe != string.Empty)
                Response.Cookies.Append("RememberMe", tokenModel.RememberMe, _cookieOpts.GetRememberOptions());

            Response.Cookies.Append("JWToken", tokenModel.JWT, _cookieOpts.GetAccessOptions());

            return RedirectToAction("Dashboard", "TaskHome");
        }

        [HttpGet]
        public async Task<IActionResult> LogOut()
        {
            if (Request.Cookies.TryGetValue("RememberMe", out string? token))
            {
                Response.Cookies.Delete("RememberMe");

                if(!string.IsNullOrEmpty(token))
                    await _tokenHandler.DeleteRememberToken(token);
            }

            Response.Cookies.Delete("JWToken");

            return RedirectToAction("Login");
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

            userSignIn.User!.Password = BCrypt.Net.BCrypt.HashPassword(userSignIn.User!.Password);

            try
            {
                await _authService.Register(userSignIn.User!);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred in endpoint POST .../Auth/SignIn");
            }
            
            ViewData["SignInMessage"] = "Your registration was successful!";

            return View("Login");
        }
    }
}
