using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CafeteriaApp.Models;

namespace CafeteriaApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Menu"); // Or wherever you want users to go after logout
        }
    }
}
