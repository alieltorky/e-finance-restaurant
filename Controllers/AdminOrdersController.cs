using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;

namespace Online_Restaurant.Controllers
{
    public class AdminOrdersController : Controller
    {
        private readonly AppdbContext _context;
        private const int PreparingStatusId = 4; // "accepted" trigger for inventory deduction
        private const int InternalUseSupplierId = 4;

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

            ViewBag.AllStatuses = await _context.OrderStatuses.ToListAsync();

            return View(orders);
        }

        // POST: AdminOrders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, int orderStatusId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Inventories)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            bool movingToPreparing = orderStatusId == PreparingStatusId
                                      && order.OrderStatusId != PreparingStatusId;
            bool alreadyDeducted = order.Inventories.Any();

            order.OrderStatusId = orderStatusId;

            if (movingToPreparing && !alreadyDeducted)
            {
                var menuItemIds = order.OrderDetails
                    .Select(d => d.Menu_ItemId)
                    .Distinct()
                    .ToList();

                var menuIngredients = await _context.MenuIngredients
                    .Where(mi => menuItemIds.Contains(mi.Menu_ItemId))
                    .ToListAsync();

                foreach (var detail in order.OrderDetails)
                {
                    var neededIngredients = menuIngredients
                        .Where(mi => mi.Menu_ItemId == detail.Menu_ItemId);

                    foreach (var mi in neededIngredients)
                    {
                        decimal usedQty = mi.Quantity * detail.Quantity;

                        _context.Inventories.Add(new Inventory
                        {
                            IngredientId = mi.IngredientId,
                            OrderId = order.OrderId,
                            SupplierId = InternalUseSupplierId,
                            DeliveryDate = DateTime.Now,
                            Quantity = -usedQty,
                            Cost = 0
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Order #{orderId} status updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}