using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeteriaApp.Data;
using CafeteriaApp.Models;

[Authorize]
public class CartController : Controller
{
    private readonly CafeteriaContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(CafeteriaContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Show menu items for ordering
    [HttpGet]
    public IActionResult Create()
    {
        var menuItems = _context.MenuItems.ToList();
        return View(menuItems);
    }

    // POST: Update quantities for items already in cart ONLY
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Dictionary<int, int> itemQuantities)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (cart == null)
        {
            return RedirectToAction("ViewCart");
        }

        foreach (var entry in itemQuantities.Where(e => e.Value > 0))
        {
            var existingItem = cart.Items.FirstOrDefault(i => i.MenuItemId == entry.Key);
            if (existingItem != null)
            {
                existingItem.Quantity = entry.Value;
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("ViewCart");
    }

    // POST: Add item to cart (or increase quantity)

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int menuItemId, int quantity = 1)
    {
        if (quantity <= 0) quantity = 1;

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (cart == null)
        {
            cart = new Cart { UserId = user.Id };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        var existingItem = cart.Items.FirstOrDefault(i => i.MenuItemId == menuItemId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                MenuItemId = menuItemId,
                Quantity = quantity
            });
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Item added to cart successfully!";

        return RedirectToAction("MenuList", "Menu");
    }

    // POST: Remove item from cart
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromCart(int menuItemId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (cart == null)
            return RedirectToAction("ViewCart");

        var item = cart.Items.FirstOrDefault(i => i.MenuItemId == menuItemId);
        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("ViewCart");
    }

    // GET: View current cart contents
    [HttpGet]
    public async Task<IActionResult> ViewCart()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        return View(cart);
    }

    // POST: Checkout - convert cart to order and clear cartccount");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);
    
        if (cart == null || !cart.Items.Any())
            return RedirectToAction("ViewCart");
    
        var order = new Order
        {
            UserId = user.Id,
            OrderDate = DateTime.Now,
            Items = cart.Items.Select(ci => new OrderItem
            {
                MenuItemId = ci.MenuItemId,
                Quantity = ci.Quantity
            }).ToList()
        };
    
        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(cart.Items);
        await _context.SaveChangesAsync();
    
        TempData["SuccessMessage"] = "Checkout successful! Your order has been placed.";
    
        return RedirectToAction("Confirmation", "Order");
    }


    // GET: Show confirmation page with last order details
    [HttpGet]
    public async Task<IActionResult> Confirmation()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var latestOrder = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.MenuItem)
            .Where(o => o.UserId == user.Id)
            .OrderByDescending(o => o.OrderDate)
            .FirstOrDefaultAsync();

        if (latestOrder == null)
            return RedirectToAction("Create");

        return View(latestOrder);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQuantityModel model)
    {
        // Find cart and item, update quantity, save changes
        var cart = await GetCartForCurrentUserAsync();
        if (cart == null)
        {
            return Json(new { success = false });
        }
        var item = cart.Items.FirstOrDefault(i => i.MenuItemId == model.MenuItemId);
        if (item != null && model.Quantity > 0)
        {
            item.Quantity = model.Quantity;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        return Json(new { success = false });
    }
    
    public class UpdateQuantityModel
    {
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
    }

    // Helper method to get the current user's cart
    private async Task<Cart?> GetCartForCurrentUserAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return null;
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);
    }
}
