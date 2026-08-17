using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;

[Authorize(Roles = "Admin")]
public class SuppliersController : Controller
{
    private readonly AppdbContext _context;

    public SuppliersController(AppdbContext context)
    {
        _context = context;
    }

    // GET all Suppliers or edited supplier
    public async Task<IActionResult> Index(int? editId)
    {
        var suppliers = await _context.Suppliers.ToListAsync();
        //taken from view (supplierToEdit)
        Supplier? supplierToEdit = null;

        if (editId.HasValue)
        {
            supplierToEdit = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierId == editId.Value);
        }

        ViewBag.SupplierToEdit = supplierToEdit;

        return View(suppliers);
    }


    // POST Create Suppliers
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Supplier supplier)
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

        _context.Suppliers.Add(supplier);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: Edit Suppliers
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Supplier supplier)
    {
        if (!ModelState.IsValid)
        {
            var suppliers = await _context.Suppliers.ToListAsync();

            ViewBag.SupplierToEdit = supplier;

            return View("Index", suppliers);
        }

        var existingSupplier = await _context.Suppliers
            .FindAsync(supplier.SupplierId);

        if (existingSupplier == null)
        {
            return NotFound();
        }

        // Update existing data
        existingSupplier.CompanyName = supplier.CompanyName;
        existingSupplier.ContactPerson = supplier.ContactPerson;
        existingSupplier.Phone = supplier.Phone;
        existingSupplier.Email = supplier.Email;
        existingSupplier.Address = supplier.Address;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // POST: Delete Suppliers
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);

        if (supplier != null)
        {
            _context.Suppliers.Remove(supplier);

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}