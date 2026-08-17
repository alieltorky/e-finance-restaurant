namespace Online_Restaurant.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;

[Authorize(Roles = "Admin")]
public class IngredientsController : Controller
{
    private readonly AppdbContext _context;

    public IngredientsController(AppdbContext context)
    {
        _context = context;
    }

    // GET: Ingredients
    public async Task<IActionResult> Index(int? editId)
    {
        var ingredients = await _context.Ingredients
            .ToListAsync();

        Ingredient? ingredientToEdit = null;

        if (editId.HasValue)
        {
            ingredientToEdit = await _context.Ingredients
                .FirstOrDefaultAsync(i => i.IngredientId == editId.Value);
        }

        ViewBag.IngredientToEdit = ingredientToEdit;

        return View(ingredients);
    }

    // POST: Create ingredient
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Ingredient ingredient)
    {
        if (!ModelState.IsValid)
        {
            var ingredients = await _context.Ingredients
                .ToListAsync();

            ViewBag.IngredientToEdit = null;

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
        if (!ModelState.IsValid)
        {
            var ingredients = await _context.Ingredients
                .ToListAsync();

            ViewBag.IngredientToEdit = ingredient;

            return View("Index", ingredients);
        }

        var existingIngredient = await _context.Ingredients
            .FindAsync(ingredient.IngredientId);

        if (existingIngredient == null)
        {
            return NotFound();
        }

        existingIngredient.IngredientName = ingredient.IngredientName;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // POST: Delete ingredient
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var ingredient = await _context.Ingredients
            .FindAsync(id);

        if (ingredient != null)
        {
            _context.Ingredients.Remove(ingredient);

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}