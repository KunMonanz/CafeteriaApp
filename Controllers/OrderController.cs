using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CafeteriaApp.Data;
using CafeteriaApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CafeteriaApp.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly CafeteriaContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(CafeteriaContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Display menu items with quantity inputs
        [HttpGet]
        public IActionResult Create()
        {
            var menuItems = _context.MenuItems.ToList();
            return View(menuItems);
        }

        // POST: Handle order form submission
        [HttpPost]
        public async Task<IActionResult> Create(Dictionary<int, int> itemQuantities)
        {
            var user = await _userManager.GetUserAsync(User);

            var order = new Order
            {
                UserId = user!.Id,
                OrderDate = DateTime.Now
            };

            // Add only items with quantity > 0
            foreach (var entry in itemQuantities.Where(e => e.Value > 0))
            {
                order.Items.Add(new OrderItem
                {
                    MenuItemId = entry.Key,
                    Quantity = entry.Value
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Confirmation");
        }

        // GET: Show confirmation and total cost
        public async Task<IActionResult> Confirmation()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Create");
            }

            var latestOrder = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.MenuItem)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .FirstOrDefaultAsync();

            if (latestOrder == null)
            {
                return RedirectToAction("Create");
            }

            return View(latestOrder);
        }
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Create");
            }
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.MenuItem)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
    }
}
