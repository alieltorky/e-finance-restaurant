using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;
using System.Security.Claims;

namespace Online_Restaurant.Controllers
{
    public class AccountController : Controller
    {
        //private readonly AppdbContext _context;
        //private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
       private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

            
        public AccountController(UserManager<ApplicationUser> userManager,
                                SignInManager<ApplicationUser> signInManager,
                                RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // GET: Account/IsAuthenticated
        // Lightweight check used by JS (e.g. Cart page) before checkout
        [HttpGet]
        public IActionResult IsAuthenticated()
        {
            var userId = User.Identity.IsAuthenticated
                ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            return Json(new { isAuthenticated = User.Identity.IsAuthenticated, userId });
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string userName, string email, string password, string confirmPassword, string? address, string? returnUrl = null)
        {
            // Role is assigned by the server below, not submitted by the form
            ModelState.Remove("Role");

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                Address = address
            };
            // Creates user, hashes password automatically, and checks uniqueness/rules
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Customer"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Customer"));
                }
                // Assign default role to self-registered accounts
                await _userManager.AddToRoleAsync(user, "Customer");

                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToLocal(returnUrl);

            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();

        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe, string? returnUrl = null)
        {
            if(!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
            // Find user by Email first
            var user = await _userManager.FindByEmailAsync(email) ?? await _userManager.FindByNameAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
            // Verify password and sign in (lockoutOnFailure: false for now)
            var result = await _signInManager.PasswordSignInAsync(user.UserName!, password, rememberMe, lockoutOnFailure: false);

            if(result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            ModelState.AddModelError("", "Invalid email or password.");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        // GET: Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        
        // Only redirect within this site — blocks an attacker from
        // injecting an external URL via ?returnUrl=
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "MenuItemsController1");
        }
    }
}