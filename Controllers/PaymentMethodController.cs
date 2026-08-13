using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;

public class PaymentMethodController : Controller
{
    private readonly AppdbContext _context;

    public PaymentMethodController(AppdbContext context)
    {
        _context = context;
    }

    // GET all Payment Methods or edited payment method
    public async Task<IActionResult> Index(int? editId)
    {
        var paymentMethods = await _context.PaymentMethods.ToListAsync();

        PaymentMethod? paymentMethodToEdit = null;

        if (editId.HasValue)
        {
            paymentMethodToEdit = await _context.PaymentMethods
                .FirstOrDefaultAsync(p => p.PaymentMethodId == editId.Value);
        }

        ViewBag.PaymentMethodToEdit = paymentMethodToEdit;

        return View(paymentMethods);
    }
    //return the payment methods to the JS pop up in user checkout
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var paymentMethods = await _context.PaymentMethods
            .Select(p => new { p.PaymentMethodId, p.MethodName })
            .ToListAsync();

        return Json(paymentMethods);
    }
    // POST: Create Payment Method
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PaymentMethod paymentMethod)
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

        _context.PaymentMethods.Add(paymentMethod);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: Edit Payment Method
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PaymentMethod paymentMethod)
    {
        if (!ModelState.IsValid)
        {
            var paymentMethods = await _context.PaymentMethods.ToListAsync();
            ViewBag.PaymentMethodToEdit = paymentMethod;
            return View("Index", paymentMethods);
        }

        var existingPaymentMethod = await _context.PaymentMethods
            .FindAsync(paymentMethod.PaymentMethodId);

        if (existingPaymentMethod == null)
        {
            return NotFound();
        }

        existingPaymentMethod.MethodName = paymentMethod.MethodName;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // POST: Delete Payment Method
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var paymentMethod = await _context.PaymentMethods.FindAsync(id);

        if (paymentMethod != null)
        {
            bool isInUse = await _context.Orders.AnyAsync(o => o.PaymentMethodId == id);

            if (isInUse)
            {
                TempData["Error"] = "This payment method is used by existing orders and cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            _context.PaymentMethods.Remove(paymentMethod);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}