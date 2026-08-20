namespace Online_Restaurant.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;

[Authorize(Roles = "Admin")]
public class MenuItemsController1 : Controller
{
    private readonly AppdbContext _context;
    private readonly IWebHostEnvironment _environment;

    public MenuItemsController1(
        AppdbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // Show the menu
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

    // Show the menu for admin
    [HttpGet]
    public async Task<IActionResult> AdminMenu()
    {
        var categories = await _context.Categories
            .Include(c => c.MenuItems)
            .ToListAsync();

        ViewBag.IsAdmin = true;

        return View("Index", categories);
    }

    // Create page
    [HttpGet]
    public async Task<IActionResult> Create(int categoryId)
    {
        var category = await _context.Categories
            .FindAsync(categoryId);

        ViewBag.CategoryId = categoryId;
        ViewBag.CategoryName = category?.CategoryName ?? "Category";

        return View();
    }

    // Add new menu item
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Menu_Item menuItem,
        IFormFile? imageFile)
    {
        ModelState.Remove("Category");
        ModelState.Remove("Menu_Ingredients");
        ModelState.Remove("OrderDetails");
        ModelState.Remove("ImagePath");

        if (string.IsNullOrWhiteSpace(menuItem.Description))
        {
            menuItem.Description = "N/A";
        }

        // Check the image before saving it
        if (imageFile != null && imageFile.Length > 0)
        {
            ValidateImage(imageFile);
        }

        if (!ModelState.IsValid)
        {
            var category = await _context.Categories
                .FindAsync(menuItem.CategoryId);

            ViewBag.CategoryName =
                category?.CategoryName ?? "Category";

            // Ingredients are shown read-only here; only the Chef role
            // can add/edit/remove them (see ChefController).
            return View(menuItem);
        }

        // Update menu item

        // Use the default image if no image was selected
        if (imageFile != null && imageFile.Length > 0)
        {
            menuItem.ImagePath = await SaveImage(imageFile);
        }
        else
        {
            menuItem.ImagePath = "/images/1.png";
        }

        _context.MenuItems.Add(menuItem);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Item added successfully!";

        return RedirectToAction("AdminMenu");
    }

    // Edit page
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
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

        ViewBag.CategoryName =
            category?.CategoryName ?? "Category";

        ViewBag.AllIngredients =
            await _context.Ingredients.ToListAsync();

        return View(menuItem);
    }

    // Update menu item
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        Menu_Item menuItem,
        IFormFile? imageFile)
    {
        if (id != menuItem.MenuItemId)
        {
            return NotFound();
        }

        ModelState.Remove("Category");
        ModelState.Remove("Menu_Ingredients");
        ModelState.Remove("OrderDetails");
        ModelState.Remove("ImagePath");

        if (string.IsNullOrWhiteSpace(menuItem.Description))
        {
            menuItem.Description = "N/A";
        }

        // Validate the new image if there is one
        if (imageFile != null && imageFile.Length > 0)
        {
            ValidateImage(imageFile);
        }

        if (!ModelState.IsValid)
        {
            var category = await _context.Categories
                .FindAsync(menuItem.CategoryId);

            ViewBag.CategoryName =
                category?.CategoryName ?? "Category";

            return View(menuItem);
        }

        var existingItem = await _context.MenuItems
            .FindAsync(id);

        if (existingItem == null)
        {
            return NotFound();
        }

        existingItem.Name = menuItem.Name;
        existingItem.Price = menuItem.Price;
        existingItem.Description = menuItem.Description;
        existingItem.Available = menuItem.Available;

        // Only change the image if a new one was uploaded
        if (imageFile != null && imageFile.Length > 0)
        {
            DeleteImage(existingItem.ImagePath);

            existingItem.ImagePath = await SaveImage(imageFile);
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Item updated successfully!";

        return RedirectToAction("AdminMenu");
    }

    // Delete menu item
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var menuItem = await _context.MenuItems
            .FindAsync(id);

        if (menuItem == null)
        {
            TempData["ErrorMessage"] = "Item not found!";

            return RedirectToAction("AdminMenu");
        }

        DeleteImage(menuItem.ImagePath);

        _context.MenuItems.Remove(menuItem);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Item deleted successfully!";

        return RedirectToAction("AdminMenu");
    }

    // Check image type and size
    private void ValidateImage(IFormFile imageFile)
    {
        var allowedExtensions = new[]
        {
            ".jpg",
            ".jpeg",
            ".png"
        };

        string extension =
            Path.GetExtension(imageFile.FileName)
                .ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            ModelState.AddModelError(
                "imageFile",
                "Only JPG, JPEG and PNG images are allowed."
            );
        }

        const long maxSize = 2 * 1024 * 1024;

        if (imageFile.Length > maxSize)
        {
            ModelState.AddModelError(
                "imageFile",
                "Image size cannot exceed 2 MB."
            );
        }
    }

    // Save the image in wwwroot/images
    private async Task<string> SaveImage(IFormFile imageFile)
    {
        string imagesFolder = Path.Combine(
            _environment.WebRootPath,
            "images"
        );

        if (!Directory.Exists(imagesFolder))
        {
            Directory.CreateDirectory(imagesFolder);
        }

        string extension =
            Path.GetExtension(imageFile.FileName)
                .ToLowerInvariant();

        string fileName = $"{Guid.NewGuid()}{extension}";

        string filePath =
            Path.Combine(imagesFolder, fileName);

        using var stream =
            new FileStream(filePath, FileMode.Create);

        await imageFile.CopyToAsync(stream);

        return $"/images/{fileName}";
    }

    // Remove the old image
    private void DeleteImage(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
        {
            return;
        }

        // Keep the default image
        if (imagePath == "/images/1.png")
        {
            return;
        }

        string filePath = Path.Combine(
            _environment.WebRootPath,
            imagePath.TrimStart('/')
        );

        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }
    }
    // Ingredient management (add/edit/delete Menu_Ingredients) has moved
    // to ChefController — only the Chef role may change an item's recipe.
}