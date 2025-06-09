using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CafeteriaApp.Data;
using CafeteriaApp.Models;
using Microsoft.AspNetCore.Identity;

namespace CafeteriaApp.Controllers
{
    [Authorize]
    public class MenuController : Controller
    {
        private readonly CafeteriaContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env; // ✅ Now inside the class

        public MenuController(CafeteriaContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env; // ✅ assign injected value
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> MenuList()
        {
            var items = _context.MenuItems.ToList();

            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = false;
            if (user != null)
            {
                isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            }

            ViewBag.IsAdmin = isAdmin;
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int itemid)
        {
            var item = await _context.MenuItems.FindAsync(itemid);
            if (item == null)
            {
                return NotFound();
            }

            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction("MenuList");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MenuItem updatedItem, IFormFile imageFile)
        {
            if (id != updatedItem.MenuItemId)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existingItem = await _context.MenuItems.FindAsync(id);
                if (existingItem == null)
                    return NotFound();

                existingItem.Name = updatedItem.Name;
                existingItem.Price = updatedItem.Price;
                existingItem.Description = updatedItem.Description;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "menu");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    existingItem.ImagePath = "/images/" + uniqueFileName;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Menu item updated successfully!";
                return RedirectToAction("MenuList");
            }

            return View(updatedItem);
        }
    }
}
