using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;
using Online_Restaurant.ViewModels;

namespace Online_Restaurant.Controllers
{
    [Authorize(Roles = "Admin")]
    public class InventoryController : Controller
    {
        private readonly AppdbContext _context;

        private const int PageSize = 10;

        public InventoryController(AppdbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            var query = await _context.Inventories
                .Include(i => i.Ingredient)
                .ToListAsync();
            var groupedInventory = query
                .GroupBy(i => new
                {
                    i.IngredientId
                })
                .Select(g => new Inventory
                {
                    IngredientId = g.Key.IngredientId,
                    Ingredient = g.First().Ingredient,
                    Quantity = g.Sum(i => i.Quantity),

                    // Optional: sum all costs
                    Cost = g.Sum(i => i.Cost)
                })
                .OrderBy(i => i.Ingredient.IngredientName)
                .ToList();

            int totalCount = groupedInventory.Count;

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(totalCount / (double)PageSize)
            );

            if (pageNumber < 1)
                pageNumber = 1;

            if (pageNumber > totalPages)
                pageNumber = totalPages;

            var inventory = groupedInventory
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Suppliers dropdown
            ViewBag.Suppliers = await _context.Suppliers
                .ToListAsync();

            // Ingredients dropdown
            ViewBag.Ingredients = await _context.Ingredients
                .ToListAsync();

            var viewModel = new InventoryIndexViewModel
            {
                InventoryRecords = inventory,
                PageNumber = pageNumber,
                TotalPages = totalPages
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Inventory inventory)
        {
            // Supplier and Ingredient are navigation properties.
            // They are not submitted by the HTML form.
            // Only SupplierId and IngredientId are submitted.

            ModelState.Remove("Supplier");
            ModelState.Remove("Ingredient");


            // Check validation

            if (!ModelState.IsValid)
            {
                // Reload Suppliers dropdown

                ViewBag.Suppliers = await _context.Suppliers
                    .ToListAsync();


                // Reload Ingredients dropdown

                ViewBag.Ingredients = await _context.Ingredients
                    .ToListAsync();


                // Reload existing inventory records (first page)

                var query = _context.Inventories
                    .Include(i => i.Supplier)
                    .Include(i => i.Ingredient)
                    .OrderByDescending(i => i.Id);

                int totalCount = await query.CountAsync();

                int totalPages = Math.Max(
                    1,
                    (int)Math.Ceiling(totalCount / (double)PageSize));

                var inventories = await query
                    .Take(PageSize)
                    .ToListAsync();

                var viewModel = new InventoryIndexViewModel
                {
                    InventoryRecords = inventories,
                    PageNumber = 1,
                    TotalPages = totalPages
                };

                return View("Index", viewModel);
            }


            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s =>
                    s.SupplierId == inventory.SupplierId);


            if (supplier == null)
            {
                return NotFound("Supplier was not found.");
            }


            var ingredient = await _context.Ingredients
                .FirstOrDefaultAsync(i =>
                    i.IngredientId == inventory.IngredientId);


            if (ingredient == null)
            {
                return NotFound("Ingredient was not found.");
            }

            // Transaction

            var strategy =
                _context.Database.CreateExecutionStrategy();


            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    // Start transaction INSIDE execution strategy

                    await using var transaction =
                        await _context.Database
                            .BeginTransactionAsync();

                    try
                    {
                        // Add the new inventory record

                        _context.Inventories.Add(inventory);


                        // Save changes

                        await _context.SaveChangesAsync();


                        // Commit transaction

                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        // Undo changes if something fails

                        await transaction.RollbackAsync();

                        throw;
                    }
                });


                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return Content(
                    "Error happened! Inventory record could not be created."
                );
            }
        }
    }
}