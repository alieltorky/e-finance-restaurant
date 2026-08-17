namespace Online_Restaurant.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;

[Authorize(Roles = "Admin")]
public class MenuItemsController1 : Controller

{ //public IActionResult Index()
  //{
  //    return View();
  //}
    private readonly AppdbContext _context;

    
    public MenuItemsController1(AppdbContext context)
    {
        _context = context;
    }
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Include(c => c.MenuItems)
            .ToListAsync();

        ViewBag.IsAdmin = false;
        return View(categories);
    }
    [HttpGet]
    public async Task<IActionResult> AdminMenu()
    {
        var categories = await _context.Categories
            .Include(c => c.MenuItems)
            .ToListAsync();

        ViewBag.IsAdmin = true;
        return View("Index", categories);
    }
    // GET: Create
    [HttpGet]
    public async Task<IActionResult> Create(int categoryId)
    {
        // Retrieve Category Name to display in the UI
        var category = await _context.Categories.FindAsync(categoryId);

        ViewBag.CategoryId = categoryId;
        ViewBag.CategoryName = category != null ? category.CategoryName : "Selected Category";

        return View();
    }


    // POST: Create Menu Item
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Menu_Item menuItem)
    {
        // Remove navigation property validation error if entity framework requires it
        ModelState.Remove("Category");
        if (string.IsNullOrWhiteSpace(menuItem.Description))
        {
            menuItem.Description = "N/A";
            ModelState.Remove("Description");
        }

        if (ModelState.IsValid)
        {
            // Add new item to database and save changes
            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            // Redirect to admin menu page after successful insert
            return RedirectToAction("AdminMenu", "MenuItemsController1");
        }

        // Reload category information if model state validation fails
        var category = await _context.Categories.FindAsync(menuItem.CategoryId);
        ViewBag.CategoryId = menuItem.CategoryId;
        ViewBag.CategoryName = category != null ? category.CategoryName : "Selected Category";

        return View(menuItem);
    }
    // POST: Delete Menu Item
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var menuItem = await _context.MenuItems.FindAsync(id);
        if (menuItem != null)
        {
            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();

            // Store success message in TempData
            TempData["SuccessMessage"] = "Item deleted successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = "Item not found!";
        }

        // Redirect back to刷新 the same page
        return RedirectToAction("AdminMenu");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, int? editMenuIngredientId)
    {
        var menuItem = await _context.MenuItems
            .Include(m => m.Menu_Ingredients)
                .ThenInclude(mi => mi.Ingredient)
            .FirstOrDefaultAsync(m => m.MenuItemId == id);

        if (menuItem == null)
        {
            return NotFound();
        }

        // Retrieve Category Name for display
        var category = await _context.Categories.FindAsync(menuItem.CategoryId);
        ViewBag.CategoryName = category != null ? category.CategoryName : "Category";

        // All ingredients available, used to populate the "add ingredient" dropdown
        ViewBag.AllIngredients = await _context.Ingredients.ToListAsync();

        // If editing a specific menu ingredient's quantity, load it for the inline edit form
        if (editMenuIngredientId.HasValue)
        {
            ViewBag.MenuIngredientToEdit = menuItem.Menu_Ingredients
                .FirstOrDefault(mi => mi.MenuIngredientId == editMenuIngredientId.Value);
        }

        return View(menuItem);
    }

    // POST: MenuItemsController1/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Menu_Item menuItem)
    {
        if (id != menuItem.MenuItemId)
        {
            return NotFound();
        }

        // Clear navigation properties validation errors
        ModelState.Remove("Category");
        ModelState.Remove("Menu_Ingredients");
        ModelState.Remove("OrderDetails");

        // Handle optional Description logic
        if (string.IsNullOrWhiteSpace(menuItem.Description))
        {
            menuItem.Description = "N/A";
            ModelState.Remove("Description");
        }

        if (ModelState.IsValid)
        {
            // 1. Fetch existing item from database
            var existingMenuItem = await _context.MenuItems.FindAsync(id);

            if (existingMenuItem == null)
            {
                return NotFound();
            }

            // 2. Explicitly update properties
            existingMenuItem.Name = menuItem.Name;
            existingMenuItem.Price = menuItem.Price;
            existingMenuItem.Available = menuItem.Available;
            existingMenuItem.Description = menuItem.Description;

            // 3. Save changes safely
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Item updated successfully!";
            // Redirect to AdminMenu in MenuItemsController1
            return RedirectToAction("AdminMenu", "MenuItemsController1");
        }

        // Reload category information if validation fails
        var category = await _context.Categories.FindAsync(menuItem.CategoryId);
        ViewBag.CategoryName = category != null ? category.CategoryName : "Category";

        return View(menuItem);
    }

    // GET: JSON list of ingredients currently linked to this menu item
    [HttpGet]
    public async Task<IActionResult> GetMenuIngredients(int menuItemId)
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

    // POST: add a new ingredient to this menu item
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMenuIngredient(int menuItemId, int ingredientId, decimal quantity)
    {
        var menuItemEntity = await _context.MenuItems.FindAsync(menuItemId);

        if (menuItemEntity == null)
        {
            return Json(new { success = false, message = "Menu item not found." });
        }

        bool alreadyLinked = await _context.MenuIngredients
            .AnyAsync(mi => mi.Menu_ItemId == menuItemId && mi.IngredientId == ingredientId);

        if (alreadyLinked)
        {
            return Json(new { success = false, message = "This ingredient is already added to this menu item." });
        }

        var menuIngredient = new Menu_Ingredient
        {
            Menu_ItemId = menuItemId,
            Menu_Item = menuItemEntity,
            IngredientId = ingredientId,
            Quantity = quantity
        };

        _context.MenuIngredients.Add(menuIngredient);
        await _context.SaveChangesAsync();

        var ingredient = await _context.Ingredients.FindAsync(ingredientId);

        return Json(new
        {
            success = true,
            menuIngredient.MenuIngredientId,
            menuIngredient.IngredientId,
            IngredientName = ingredient?.IngredientName,
            menuIngredient.Quantity
        });
    }

    // POST: update the quantity of an existing menu ingredient
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMenuIngredient(int menuIngredientId, decimal quantity)
    {
        var menuIngredient = await _context.MenuIngredients.FindAsync(menuIngredientId);

        if (menuIngredient == null)
        {
            return Json(new { success = false, message = "Ingredient not found." });
        }

        menuIngredient.Quantity = quantity;
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    // POST: remove an ingredient from this menu item
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMenuIngredient(int menuIngredientId)
    {
        var menuIngredient = await _context.MenuIngredients.FindAsync(menuIngredientId);

        if (menuIngredient != null)
        {
            _context.MenuIngredients.Remove(menuIngredient);
            await _context.SaveChangesAsync();
        }

        return Json(new { success = true });
    }


}