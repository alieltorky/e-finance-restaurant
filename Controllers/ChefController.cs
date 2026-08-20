namespace Online_Restaurant.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;

// Only the Chef role may view/change a menu item's ingredients (recipe).
[Authorize(Roles = "Chef")]
public class ChefController : Controller
{
    private readonly AppdbContext _context;

    public ChefController(AppdbContext context)
    {
        _context = context;
    }

    // Chef dashboard: browse categories/menu items to pick one to manage
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Include(c => c.MenuItems)
                .ThenInclude(m => m.Menu_Ingredients)
            .ToListAsync();

        return View(categories);
    }

    // Ingredient management page for a single menu item
    [HttpGet]
    public async Task<IActionResult> Ingredients(int id)
    {
        var menuItem = await _context.MenuItems
            .Include(m => m.Menu_Ingredients)
                .ThenInclude(mi => mi.Ingredient)
            .FirstOrDefaultAsync(m => m.MenuItemId == id);

        if (menuItem == null)
        {
            return NotFound();
        }

        var category = await _context.Categories
            .FindAsync(menuItem.CategoryId);

        ViewBag.CategoryName = category?.CategoryName ?? "Category";

        ViewBag.AllIngredients =
            await _context.Ingredients.ToListAsync();

        return View(menuItem);
    }

    // Get ingredients for a menu item
    [HttpGet]
    public async Task<IActionResult> GetMenuIngredients(
        int menuItemId)
    {
        var ingredients = await _context.MenuIngredients
            .Where(mi => mi.Menu_ItemId == menuItemId)
            .Include(mi => mi.Ingredient)
            .Select(mi => new
            {
                mi.MenuIngredientId,
                mi.IngredientId,
                IngredientName = mi.Ingredient.IngredientName,
                mi.Quantity
            })
            .ToListAsync();

        return Json(ingredients);
    }

    // Add ingredient to a menu item
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMenuIngredient(
        int menuItemId,
        int ingredientId,
        decimal quantity)
    {
        var menuItem = await _context.MenuItems
            .FindAsync(menuItemId);

        if (menuItem == null)
        {
            return Json(new
            {
                success = false,
                message = "Menu item not found."
            });
        }

        bool alreadyExists =
            await _context.MenuIngredients
                .AnyAsync(mi =>
                    mi.Menu_ItemId == menuItemId &&
                    mi.IngredientId == ingredientId);

        if (alreadyExists)
        {
            return Json(new
            {
                success = false,
                message = "This ingredient is already added."
            });
        }

        var menuIngredient = new Menu_Ingredient
        {
            Menu_ItemId = menuItemId,
            IngredientId = ingredientId,
            Quantity = quantity
        };

        _context.MenuIngredients.Add(menuIngredient);

        await _context.SaveChangesAsync();

        var ingredient = await _context.Ingredients
            .FindAsync(ingredientId);

        return Json(new
        {
            success = true,
            menuIngredient.MenuIngredientId,
            menuIngredient.IngredientId,
            IngredientName = ingredient?.IngredientName,
            menuIngredient.Quantity
        });
    }

    // Change ingredient quantity
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMenuIngredient(
        int menuIngredientId,
        decimal quantity)
    {
        var menuIngredient =
            await _context.MenuIngredients
                .FindAsync(menuIngredientId);

        if (menuIngredient == null)
        {
            return Json(new
            {
                success = false,
                message = "Ingredient not found."
            });
        }

        menuIngredient.Quantity = quantity;

        await _context.SaveChangesAsync();

        return Json(new
        {
            success = true
        });
    }

    // Remove ingredient from menu item
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMenuIngredient(
        int menuIngredientId)
    {
        var menuIngredient =
            await _context.MenuIngredients
                .FindAsync(menuIngredientId);

        if (menuIngredient != null)
        {
            _context.MenuIngredients.Remove(menuIngredient);

            await _context.SaveChangesAsync();
        }

        return Json(new
        {
            success = true
        });
    }
}