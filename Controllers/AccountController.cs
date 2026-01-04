using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using WarehouseMvc.Data;       // DbContext (WarehouseContext)
using WarehouseMvc.Models;     // LoginViewModel, AppUser

namespace WarehouseMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly WarehouseContext _context;

        public AccountController(WarehouseContext context)
        {
            _context = context;
        }

      
        //        LOGIN
        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;// redirect back to the home page 
            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Look for a matching user in the Users table
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == model.UserName && u.Password == model.Password);

            if (user != null)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, user.UserName)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    });

                TempData["Toast"] = "Logged in successfully.";
                TempData["ToastType"] = "success";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid username or password.");//To ensure modle validation in both Login and Register.
            TempData["Toast"] = "Login failed.";
            TempData["ToastType"] = "danger";
            return View(model);
        }

       
        //       REGISTER
        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());   // reuse same fields (UserName, Password, RememberMe)
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if username already exists
            bool exists = await _context.Users
                .AnyAsync(u => u.UserName == model.UserName);

            if (exists)
            {
                ModelState.AddModelError("UserName", "This username is already taken.");
                return View(model);
            }

            // Create new user (plain text password)
            var user = new AppUser
            {
                UserName = model.UserName,
                Password = model.Password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Auto-login the new user
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            TempData["Toast"] = "Registration successful. You are now logged in.";
            TempData["ToastType"] = "success";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        //        LOGOUT
        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["Toast"] = "Logged out.";
            TempData["ToastType"] = "info";

            return RedirectToAction("Index", "Home");
        }
    }
}
