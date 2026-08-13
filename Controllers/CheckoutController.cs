using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;
using Online_Restaurant.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Online_Restaurant.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly AppdbContext _context;
        private const int InternalUseSupplierId = 4;

        public CheckoutController(AppdbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] CheckoutRequest request)
        {
            //Check cart
            if (request == null || request.Items == null || !request.Items.Any())
            {
                return BadRequest(new { success = false, message = "Cart is empty." });
            }

            //int userId = 3; // constant user id for now
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { success = false, message = "You must be logged in to check out." });
            }

            // Check user
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest(new { success = false, message = "User does not exist." });
            }

            //check payment method
            var paymentMethod = await _context.PaymentMethods
                   .FirstOrDefaultAsync(p => p.PaymentMethodId == request.PaymentMethodId);

            if (paymentMethod == null)
            {
                return BadRequest(new { success = false, message = "Invalid payment method." });
            }
            // Get the menu items from database
            var menuItemIds = request.Items
                .Select(x => x.MenuItemId)
                .Distinct()
                .ToList();

            var menuItems = await _context.MenuItems
                .Where(x => menuItemIds.Contains(x.MenuItemId))
                .ToListAsync();

            // Check that all menu items exist
            if (menuItems.Count != menuItemIds.Count)
            {
                return BadRequest(new { success = false, message = "One or more menu items do not exist." });
            }

            // Check quantities
            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0)
                {
                    return BadRequest(new { success = false, message = "Invalid quantity." });
                }
            }

            //Pull all Menu_Ingredients needed for the ordered menu items
            var menuIngredients = await _context.MenuIngredients
                .Where(mi => menuItemIds.Contains(mi.Menu_ItemId))
                .ToListAsync();

            // Calculate order total
            decimal orderTotal = 0;
            foreach (var item in request.Items)
            {
                var menuItem = menuItems.First(x => x.MenuItemId == item.MenuItemId);
                orderTotal += menuItem.Price * item.Quantity;
            }

            //Transaction 
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync<IActionResult>(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Create Order
                    var order = new Orders
                    {
                        UserId = userId,
                        Date = DateTime.Now,
                        Price = orderTotal,
                        OrderStatusId = 4, // 4 = Preparing constant untill changed
                        PaymentMethodId = request.PaymentMethodId,
                        MobileNumber = user.PhoneNumber,
                        Address = user.Address
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync(); // Generates order.OrderId

                    // 8. Create OrderDetails
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

                    await _context.SaveChangesAsync();

                    // 9.negative Inventory rows
                    foreach (var item in request.Items)
                    {
                        var neededIngredients = menuIngredients
                            .Where(mi => mi.Menu_ItemId == item.MenuItemId);

                        foreach (var mi in neededIngredients)
                        {
                            decimal usedQty = mi.Quantity * item.Quantity;

                            var inventoryUsage = new Inventory
                            {
                                IngredientId = mi.IngredientId,
                                OrderId = order.OrderId,
                                SupplierId = InternalUseSupplierId,
                                DeliveryDate = DateTime.Now,
                                Quantity = -usedQty, 
                                Cost = 0
                            };

                            _context.Inventories.Add(inventoryUsage);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // 10. Return result
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

                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Order could not be completed. Please try again.",
                        error = ex.Message
                    });
                }
            });
        }
    }
    }