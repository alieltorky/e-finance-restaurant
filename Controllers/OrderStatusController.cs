using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;

[Authorize(Roles = "Admin")]
public class OrderStatusController : Controller
{
    private readonly AppdbContext _context;

    public OrderStatusController(AppdbContext context)
    {
        _context = context;
    }

    // GET all Order Statuses or edited order status
    public async Task<IActionResult> Index(int? editId)
    {
        var orderStatuses = await _context.OrderStatuses.ToListAsync();

        OrderStatus? orderStatusToEdit = null;

        if (editId.HasValue)
        {
            orderStatusToEdit = await _context.OrderStatuses
                .FirstOrDefaultAsync(o => o.OrderStatusId == editId.Value);
        }

        ViewBag.OrderStatusToEdit = orderStatusToEdit;

        return View(orderStatuses);
    }

    // POST: Create Order Status
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderStatus orderStatus)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Content(string.Join("\n", errors));
        }

        _context.OrderStatuses.Add(orderStatus);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: Edit Order Status
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(OrderStatus orderStatus)
    {
        if (!ModelState.IsValid)
        {
            var orderStatuses = await _context.OrderStatuses.ToListAsync();
            ViewBag.OrderStatusToEdit = orderStatus;
            return View("Index", orderStatuses);
        }

        var existingOrderStatus = await _context.OrderStatuses
            .FindAsync(orderStatus.OrderStatusId);

        if (existingOrderStatus == null)
        {
            return NotFound();
        }

        existingOrderStatus.StatusName = orderStatus.StatusName;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // POST: Delete Order Status
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var orderStatus = await _context.OrderStatuses.FindAsync(id);

        if (orderStatus != null)
        {
            bool isInUse = await _context.Orders.AnyAsync(o => o.OrderStatusId == id);

            if (isInUse)
            {
                TempData["Error"] = "This order status is used by existing orders and cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            _context.OrderStatuses.Remove(orderStatus);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}