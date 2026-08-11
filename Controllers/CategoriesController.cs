namespace Online_Restaurant.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;

public class CategoriesController : Controller
{
    private readonly AppdbContext _context;

    public CategoriesController(AppdbContext context)
    {
        _context = context;
    }

    // GET: Fetch all categories or item to edit
    public async Task<IActionResult> Index(int? editId)
    {
        var categories = await _context.Categories.ToListAsync();
        Category? categoryToEdit = null;

        if (editId.HasValue)
        {
            categoryToEdit = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == editId.Value);
        }

        ViewBag.CategoryToEdit = categoryToEdit;

        return View(categories);
    }

    // POST: Create category
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        // Ignore navigation property validation if present
        ModelState.Remove("MenuItems");

        if (!ModelState.IsValid)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.CategoryToEdit = null;
            return View("Index", categories);
        }

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: Edit category
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Category category)
    {
        // Ignore navigation property validation if present
        ModelState.Remove("MenuItems");

        if (!ModelState.IsValid)
        {
            var categories = await _context.Categories.ToListAsync();

            ViewBag.CategoryToEdit = category;

            return View("Index", categories);
        }

        var existingCategory = await _context.Categories
            .FindAsync(category.CategoryId);

        if (existingCategory == null)
        {
            return NotFound();
        }

        // Explicit property assignment
        existingCategory.CategoryName = category.CategoryName;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // POST: Delete category
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}