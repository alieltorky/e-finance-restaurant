namespace Online_Restaurant.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;

public class IngredientsController : Controller
{
    private readonly AppdbContext _context;

    public IngredientsController(AppdbContext context)
    {
        _context = context;
    }

    // GET: Fetch all ingredients or item to edit
    public async Task<IActionResult> Index(int? editId)
    {
        var ingredients = await _context.Ingredients
            .Include(i => i.Inventory)
            .ToListAsync();

        Ingredient? ingredientToEdit = null;

        if (editId.HasValue)
        {
            ingredientToEdit = await _context.Ingredients
                .FirstOrDefaultAsync(i => i.IngredientId == editId.Value);
        }

        ViewBag.IngredientToEdit = ingredientToEdit;
        ViewBag.Inventories = new SelectList(_context.Inventory, "InventoryId", "InventoryName");

        return View(ingredients);
    }

    // POST: Create ingredient
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Ingredient ingredient)
    {
        // Ignore navigation property validation
        ModelState.Remove("Inventory");

        if (!ModelState.IsValid)
        {
            var ingredients = await _context.Ingredients
                .Include(i => i.Inventory)
                .ToListAsync();

            ViewBag.IngredientToEdit = null;
            ViewBag.Inventories = new SelectList(_context.Inventory, "InventoryId", "InventoryName");

            return View("Index", ingredients);
        }

        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: Edit ingredient
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Ingredient ingredient)
    {
        // Ignore navigation property validation
        ModelState.Remove("Inventory");

        if (!ModelState.IsValid)
        {
            var ingredients = await _context.Ingredients
                .Include(i => i.Inventory)
                .ToListAsync();

            ViewBag.IngredientToEdit = ingredient;
            ViewBag.Inventories = new SelectList(_context.Inventory, "InventoryId", "InventoryName");

            return View("Index", ingredients);
        }

        var existingIngredient = await _context.Ingredients
            .FindAsync(ingredient.IngredientId);

        if (existingIngredient == null)
        {
            return NotFound();
        }

        // Explicit property assignment
        existingIngredient.IngredientName = ingredient.IngredientName;
        existingIngredient.InventoryId = ingredient.InventoryId;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // POST: Delete ingredient
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var ingredient = await _context.Ingredients.FindAsync(id);

        if (ingredient != null)
        {
            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}