using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;
using Online_Restaurant.ViewModels;

namespace Online_Restaurant.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly AppdbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const int PreparingStatusId = 4; // "accepted" trigger for inventory deduction
        private const int InternalUseSupplierId = 5;
        private const int PageSize = 10;

        public AdminOrdersController(AppdbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AdminOrders (Admin - All Orders)
        [HttpGet]
        public async Task<IActionResult> Index(string? phoneNumber, int? orderId, string? deliveryManId, int pageNumber = 1)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.DeliveryMan)
                .Include(o => o.OrderStatus)
                .Include(o => o.PaymentMethod)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                query = query.Where(o => o.MobileNumber != null && o.MobileNumber.Contains(phoneNumber));
            }

            if (orderId.HasValue)
            {
                query = query.Where(o => o.OrderId == orderId.Value);
            }

            if (!string.IsNullOrWhiteSpace(deliveryManId))
            {
                query = query.Where(o => o.DeliveryManId == deliveryManId);
            }

            query = query.OrderByDescending(o => o.Date);

            int totalCount = await query.CountAsync();
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));

            if (pageNumber < 1) pageNumber = 1;
            if (pageNumber > totalPages) pageNumber = totalPages;

            var orders = await query
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var deliveryStaff = await _userManager.GetUsersInRoleAsync("Delivery");

            var viewModel = new AdminOrdersViewModel
            {
                Orders = orders,
                AllStatuses = await _context.OrderStatuses.ToListAsync(),
                DeliveryStaff = deliveryStaff
                    .OrderBy(u => u.UserName)
                    .Select(u => new DeliveryStaffOption { Id = u.Id, Name = u.UserName ?? u.Email ?? u.Id })
                    .ToList(),
                
                PhoneNumber = phoneNumber,
                OrderId = orderId,
                DeliveryManId = deliveryManId,
                PageNumber = pageNumber,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // POST: AdminOrders/UpdateStatus
        // Accepts a JSON body: { orderId, orderStatusId }
        // Always responds with JSON: { success: bool, statusName?: string, message?: string }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Invalid request." });
            }

            int orderId = request.OrderId;
            int orderStatusId = request.OrderStatusId;

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Inventories)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound(new { success = false, message = "Order not found." });
            }

            var newStatus = await _context.OrderStatuses
                .FirstOrDefaultAsync(s => s.OrderStatusId == orderStatusId);

            if (newStatus == null)
            {
                return BadRequest(new { success = false, message = "Invalid status." });
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

            return Json(new
            {
                success = true,
                statusName = newStatus.StatusName
            });
        }
    }

    // Request model for the AJAX status update
    public class UpdateStatusRequest
    {
        public int OrderId { get; set; }
        public int OrderStatusId { get; set; }
    }
}