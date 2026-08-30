using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;
using Online_Restaurant.Services;
using Online_Restaurant.ViewModels;
using System.Text.Json; //  Added JSON serializer namespace
using Serilog; //  Added Serilog logging namespace

namespace Online_Restaurant.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly AppdbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        private const int PreparingStatusId = 4;
        private const int InternalUseSupplierId = 5;
        private const int PageSize = 10;

        public AdminOrdersController(
            AppdbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? phoneNumber,
            int? orderId,
            string? deliveryManId,
            int pageNumber = 1)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.DeliveryMan)
                .Include(o => o.OrderStatus)
                .Include(o => o.PaymentMethod)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                query = query.Where(o =>
                    o.MobileNumber != null &&
                    o.MobileNumber.Contains(phoneNumber));
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

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(totalCount / (double)PageSize));

            if (pageNumber < 1)
                pageNumber = 1;

            if (pageNumber > totalPages)
                pageNumber = totalPages;

            var orders = await query
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var deliveryStaff =
                await _userManager.GetUsersInRoleAsync("Delivery");

            var viewModel = new AdminOrdersViewModel
            {
                Orders = orders,
                AllStatuses = await _context.OrderStatuses.ToListAsync(),

                DeliveryStaff = deliveryStaff
                    .OrderBy(u => u.UserName)
                    .Select(u => new DeliveryStaffOption
                    {
                        Id = u.Id,
                        Name = u.UserName ?? u.Email ?? u.Id
                    })
                    .ToList(),

                PhoneNumber = phoneNumber,
                OrderId = orderId,
                DeliveryManId = deliveryManId,
                PageNumber = pageNumber,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            [FromBody] UpdateStatusRequest request)
        {
            // Serialized update status request data to JSON and logged incoming request
            var userName = User.Identity?.Name ?? "Guest";
            var requestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var jsonData = JsonSerializer.Serialize(request);
            Log.Information("POST-{User}-Request-{Time} | {Data}", userName, requestTime, jsonData);

            if (request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid request."
                });
            }

            int orderId = request.OrderId;
            int orderStatusId = request.OrderStatusId;

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Menu_Item)
                .Include(o => o.Inventories)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Order not found."
                });
            }

            var newStatus = await _context.OrderStatuses
                .FirstOrDefaultAsync(s =>
                    s.OrderStatusId == orderStatusId);

            if (newStatus == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid status."
                });
            }

            bool movingToPreparing = orderStatusId == PreparingStatusId && order.OrderStatusId != PreparingStatusId;

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
                        decimal usedQty =
                            mi.Quantity * detail.Quantity;

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

            // Send email when the owner accepts the order
            if (movingToPreparing)
            {
                if (order.User != null && !string.IsNullOrWhiteSpace(order.User.Email))
                {
                    var emailBody = BuildOrderEmail(order, newStatus.StatusName);

                    await _emailService.SendEmailAsync(
                        order.User.Email,
                        $"Order #{order.OrderId} Accepted",
                        emailBody);
                }
            }

            // gharbawy : Logged successful status update response
            var responseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Log.Information("POST-{User}-Response-{Time} | OrderId: {OrderId} | Status: {Status}", userName, responseTime, order.OrderId, newStatus.StatusName);

            return Json(new
            {
                success = true,
                statusName = newStatus.StatusName
            });
        }

        private string BuildOrderEmail(
            Orders order,
            string statusName)
        {
            var itemsHtml = string.Join(
                "",
                order.OrderDetails.Select(detail => $@"
                    <tr>
                        <td style='padding:8px; border-bottom:1px solid #ddd;'>
                            {detail.Menu_Item.Name}
                        </td>
                        <td style='padding:8px; border-bottom:1px solid #ddd; text-align:center;'>
                            {detail.Quantity}
                        </td>
                        <td style='padding:8px; border-bottom:1px solid #ddd; text-align:right;'>
                            ${detail.TotalPrice:F2}
                        </td>
                    </tr>
                "));

            return $@"
                <html>
                <body style='font-family:Arial,sans-serif;'>
                    <h2>Order #{order.OrderId} Accepted</h2>

                    <p>Hello {order.User.UserName},</p>

                    <p>
                        Your order has been accepted by the restaurant
                        and is now being prepared.
                    </p>

                    <h3>Order Details</h3>

                    <table style='border-collapse:collapse; width:100%; max-width:600px;'>
                        <thead>
                            <tr>
                                <th style='padding:8px; text-align:left;'>
                                    Item
                                </th>
                                <th style='padding:8px; text-align:center;'>
                                    Quantity
                                </th>
                                <th style='padding:8px; text-align:right;'>
                                    Price
                                </th>
                            </tr>
                        </thead>

                        <tbody>
                            {itemsHtml}
                        </tbody>

                        <tfoot>
                            <tr>
                                <td colspan='2'
                                    style='padding:10px; text-align:right; font-weight:bold;'>
                                    Total:
                                </td>

                                <td style='padding:10px; text-align:right; font-weight:bold;'>
                                    ${order.Price:F2}
                                </td>
                            </tr>
                        </tfoot>
                    </table>

                    <p style='margin-top:20px;'>
                        Thank you for ordering from e-Restaurant!
                    </p>
                </body>
                </html>";
        }
    }

    public class UpdateStatusRequest
    {
        public int OrderId { get; set; }
        public int OrderStatusId { get; set; }
    }
}