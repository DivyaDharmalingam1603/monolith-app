using Microsoft.AspNetCore.Mvc;
using EnergyLegacyApp.Web.Models;
using EnergyLegacyApp.Business;
using EnergyLegacyApp.Web.Helpers;
using EnergyLegacyApp.Data.Models;

namespace EnergyLegacyApp.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly AuthService _authService;
        private readonly JwtTokenGenerator _tokenGenerator;

        public AuthController(AuthService authService, JwtTokenGenerator tokenGenerator, ILogger<AuthController> logger)
        {
            _authService = authService;
            _tokenGenerator = tokenGenerator;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string message = "")
        {
            ViewBag.Message = message;
            _logger.LogInformation("Login page accessed.");
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            _logger.LogInformation("Login attempt for user: {Username}", model.Username);

            if (ModelState.IsValid)
            {
                var user = _authService.ValidateUser(model.Username, model.Password);

                if (user != null)
                {
                    var token = _tokenGenerator.GenerateToken(user.Username, user.Role);

                    Response.Cookies.Append("AuthToken", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Lax
                    });


                    _logger.LogInformation("User '{Username}' logged in successfully.", user.Username);
                    //_logger.LogInformation("token: {token}", token);
                    //return LocalRedirect(Url.Action("Index", "Home"));
                    return Redirect("/Home/Index");
                }

                _logger.LogWarning("Login failed for user: {Username} - Invalid credentials.", model.Username);
                ModelState.AddModelError("", "Invalid username or password");
            }
            else
            {
                _logger.LogWarning("Login attempt failed due to invalid model state for user: {Username}", model.Username);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            /*Response.Cookies.Delete("AuthToken");*/
            Response.Cookies.Delete("AuthToken", new CookieOptions
            {
                Path = "/", 
                SameSite = SameSiteMode.Lax,
                Secure = false
            });

            TempData.Clear();

            _logger.LogInformation("User logged out at {Time}", DateTime.Now); //Logging logout

            return RedirectToAction("Login", new { message = "You have been logged out." });
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password,
                    Email = model.Email
                };

                var success = _authService.RegisterUser(user);

                if (success)
                {
                    _logger.LogInformation("New user registered: {Username} at {Time}", model.Username, DateTime.Now); // ✅ Logging successful registration

                    TempData["Success"] = "Registration successful. Please log in.";
                    return RedirectToAction("Login");
                }

                _logger.LogWarning("Registration failed for user: {Username} - User might already exist.", model.Username); // ✅ Logging failed registration
                ModelState.AddModelError("", "Registration failed. User might already exist.");
            }
            else
            {
                _logger.LogWarning("Registration model state invalid for user: {Username}", model.Username); //Logging invalid model
            }

            return View(model);
        }
    }
}
