namespace Online_Restaurant.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;

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
    public async Task<IActionResult> Edit(int id)
    {
        var menuItem = await _context.MenuItems.FindAsync(id);
        if (menuItem == null)
        {
            return NotFound();
        }

        // Retrieve Category Name for display
        var category = await _context.Categories.FindAsync(menuItem.CategoryId);
        ViewBag.CategoryName = category != null ? category.CategoryName : "Category";

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


}
