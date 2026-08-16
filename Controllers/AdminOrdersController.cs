using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;

namespace Online_Restaurant.Controllers
{
    public class AdminOrdersController : Controller
    {
        private readonly AppdbContext _context;

        public AdminOrdersController(AppdbContext context)
        {
            _context = context;
        }

        // GET: AdminOrders (Admin - All Orders)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderStatus)
                .Include(o => o.PaymentMethod)
                .OrderByDescending(o => o.Date)
                .ToListAsync();

            // Used to populate the status dropdown for every row
            ViewBag.AllStatuses = await _context.OrderStatuses.ToListAsync();

            return View(orders);
        }

        // POST: AdminOrders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, int orderStatusId)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatusId = orderStatusId;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Order #{orderId} status updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}