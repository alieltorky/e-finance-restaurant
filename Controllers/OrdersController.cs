using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using System.Security.Claims;

namespace Online_Restaurant.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly AppdbContext _context;

        public OrdersController(AppdbContext context)
        {
            _context = context;
        }

        // GET: Orders (My Orders)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderStatus)
                .Include(o => o.PaymentMethod)
                .OrderByDescending(o => o.Date)
                .ToListAsync();

            return View(orders);
        }

        // GET: Orders/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var order = await _context.Orders
                .Include(o => o.OrderStatus)
                .Include(o => o.PaymentMethod)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Menu_Item)
                // Only ever match an order that belongs to the logged-in user
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}