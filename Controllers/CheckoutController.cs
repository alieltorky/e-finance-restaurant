using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;
using Online_Restaurant.Models.DTOs;
using System.Security.Claims;

namespace Online_Restaurant.Controllers
{
    
    public class CheckoutController : Controller
    {
        private readonly AppdbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const int InternalUseSupplierId = 4;

        public CheckoutController(AppdbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] CheckoutRequest request)
        {
            // 1. Validate request data
            if (request == null || request.Items == null || !request.Items.Any())
            {
                return BadRequest(new { success = false, message = "Cart is empty." });
            }

            // 2. Retrieve authenticated user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "You must be logged in to check out." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new { success = false, message = "User does not exist." });
            }

            // 3. Validate payment method
            var paymentMethod = await _context.PaymentMethods
                .FirstOrDefaultAsync(p => p.PaymentMethodId == request.PaymentMethodId);

            if (paymentMethod == null)
            {
                return BadRequest(new { success = false, message = "Invalid payment method." });
            }

            // 4. Validate menu items
            var menuItemIds = request.Items
                .Select(x => x.MenuItemId)
                .Distinct()
                .ToList();

            var menuItems = await _context.MenuItems
                .Where(x => menuItemIds.Contains(x.MenuItemId))
                .ToListAsync();

            if (menuItems.Count != menuItemIds.Count)
            {
                return BadRequest(new { success = false, message = "One or more menu items do not exist." });
            }

            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid quantity." });
                }
            }

            // 5. Retrieve ingredients
            var menuIngredients = await _context.MenuIngredients
                .Where(mi => menuItemIds.Contains(mi.Menu_ItemId))
                .ToListAsync();

            // 6. Calculate total price
            decimal orderTotal = 0;
            foreach (var item in request.Items)
            {
                var menuItem = menuItems.First(x => x.MenuItemId == item.MenuItemId);
                orderTotal += menuItem.Price * item.Quantity;
            }

            // 7. Execute order transaction
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Create main order record
                    var order = new Orders
                    {
                        UserId = userId,
                        Date = DateTime.Now,
                        Price = orderTotal,
                        OrderStatusId = 1, // 1:pending
                        PaymentMethodId = request.PaymentMethodId,
                        MobileNumber = user.PhoneNumber ?? "N/A",
                        Address = user.Address ?? "N/A"
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync(); // Generates order.OrderId

                    // Create order detail records
                    foreach (var item in request.Items)
                    {
                        var menuItem = menuItems.First(x => x.MenuItemId == item.MenuItemId);

                        var orderDetail = new OrderDetail
                        {
                            OrderId = order.OrderId,
                            Menu_ItemId = item.MenuItemId,
                            Quantity = item.Quantity,
                            TotalPrice = menuItem.Price * item.Quantity
                        };

                        _context.OrderDetails.Add(orderDetail);
                    }

                    // Record inventory deductions
                    //foreach (var item in request.Items)
                    //{
                    //    var neededIngredients = menuIngredients
                    //        .Where(mi => mi.Menu_ItemId == item.MenuItemId);

                    //    foreach (var mi in neededIngredients)
                    //    {
                    //        decimal usedQty = mi.Quantity * item.Quantity;

                    //        var inventoryUsage = new Inventory
                    //        {
                    //            IngredientId = mi.IngredientId,
                    //            OrderId = order.OrderId,
                    //            SupplierId = InternalUseSupplierId,
                    //            DeliveryDate = DateTime.Now,
                    //            Quantity = -usedQty,
                    //            Cost = 0
                    //        };

                    //        _context.Inventories.Add(inventoryUsage);
                    //    }
                    //}

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new
                    {
                        success = true,
                        message = "Order saved successfully.",
                        orderId = order.OrderId,
                        total = orderTotal
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    // Get detailed inner database exception
                    var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Order could not be completed. Please try again.",
                        error = realError
                    });
                }
            });
        }
    }
}