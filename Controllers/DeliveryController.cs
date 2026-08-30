using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;
using Online_Restaurant.ViewModels;

[Authorize(Roles = "Delivery")]
public class DeliveryController : Controller
{
    private readonly AppdbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeliveryController(AppdbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    // ExecuteUpdateAsync is used to solve the raceCondition prob between two deliveryMans
    {
        string currentUserId = _userManager.GetUserId(User);

        // Auto-Timer Trigger:
        // Any order in 'Preparing' (4) created more than 10 seconds ago -> becomes 'Ready' (5)
        var preparationThreshold = DateTime.Now.AddSeconds(-5); // SET TO MIN(-3)
        await _context.Orders

        .Where(o => o.OrderStatusId == 4 && o.Date <= preparationThreshold)
        .ExecuteUpdateAsync(setter => setter.SetProperty(o => o.OrderStatusId, 5));

        // Fetch available orders (Status 5 = Ready & not assigned yet)
        var availableOrders = await _context.Orders
            .Where(o => o.OrderStatusId == 5 && o.DeliveryManId == null)
            //.OrderDescending()
            .Select(o => new DeliveryOrderItemVM
            {
                OrderId = o.OrderId,
                Address = o.Address,
                MobileNumber = o.MobileNumber,
                Price = o.Price,
                IsCashOnDelivery = o.PaymentMethodId == 4, 
                OrderStatusId = o.OrderStatusId,
                Items = o.OrderDetails.Select(d => $"{d.Menu_Item.Name} ({d.Quantity})").ToList()
            })
            .ToListAsync();

        // Fetch driver's active & completed orders (Status 6 = On Delivery, 2 = Delivered)
        var myOrders = await _context.Orders
            .Where(o => o.DeliveryManId == currentUserId)
            .OrderByDescending(o => o.OrderId)
            .Select(o => new DeliveryOrderItemVM
            {
                OrderId = o.OrderId,
                Address = o.Address,
                MobileNumber = o.MobileNumber,
                Price = o.Price,
                IsCashOnDelivery = o.PaymentMethodId == 4,
                OrderStatusId = o.OrderStatusId,
                Items = o.OrderDetails.Select(d => $"{d.Menu_Item.Name} ({d.Quantity})").ToList()
            })
            .ToListAsync();

        var viewModel = new DeliveryDashboardViewModel
        {
            AvailableOrders = availableOrders,
            MyOrders = myOrders
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptOrder(int orderId)
    {
        string currentUserId = _userManager.GetUserId(User);

        // Atomic update: 5 (Ready) -> 6 (On Delivery)
        int affectedRows = await _context.Orders
            .Where(o => o.OrderId == orderId && o.DeliveryManId == null && o.OrderStatusId == 5)
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(o => o.DeliveryManId, currentUserId)
                .SetProperty(o => o.OrderStatusId, 6));

        if (affectedRows == 0)
        {
            TempData["ErrorMessage"] = "Sorry, this order was just taken by another delivery driver!";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Order assigned to you successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsDelivered(int orderId)
    {
        string currentUserId = _userManager.GetUserId(User);

        // Atomic update: 6 (On Delivery) -> 2 (Delivered)
        int affectedRows = await _context.Orders
            .Where(o => o.OrderId == orderId && o.DeliveryManId == currentUserId && o.OrderStatusId == 6)
            .ExecuteUpdateAsync(setter => setter.SetProperty(o => o.OrderStatusId, 2));

        if (affectedRows == 0)
        {
            TempData["ErrorMessage"] = "Failed to update order status.";
            return RedirectToAction("Index");
        }

        TempData["SuccessMessage"] = "Order marked as delivered successfully!";
        return RedirectToAction("Index");
    }
}