using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeteriaApp.Data;
using CafeteriaApp.Models;

namespace CafeteriaApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CafeteriaContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, CafeteriaContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Dashboard() => View();

        public async Task<IActionResult> AllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }
        public async Task<IActionResult> AllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.MenuItem)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Get all users
            var users = await _userManager.Users.ToDictionaryAsync(u => u.Id);

            // Attach emails to orders using a view model
            var model = orders.Select(order => new OrderWithUserViewModel
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                Items = order.Items.ToList(),
                UserEmail = users.ContainsKey(order.UserId) && users[order.UserId] != null ? users[order.UserId]?.Email ?? "Unknown" : "Unknown"
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(MenuItem menuItem, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Generate a unique filename
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/menu", fileName);

                    // Save the file to wwwroot/images/menu/
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Save the relative path to the database
                    menuItem.ImagePath = "/images/menu/" + fileName;
                }

                _context.MenuItems.Add(menuItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create));
            }

            return View(menuItem);

        }

        public async Task<IActionResult> MenuList()
        {
            var menuItems = await _context.MenuItems.ToListAsync();
            return View(menuItems);
        }

    }
}