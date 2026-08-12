using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;

namespace Online_Restaurant.Controllers
{
    public class InventoryController : Controller
    {
        private readonly AppdbContext _context;

        public InventoryController(AppdbContext context)
        {
            _context = context;
        }


        // =====================================================
        // GET: Inventory
        // =====================================================

        public async Task<IActionResult> Index()
        {
            // Get all inventory records
            // and load their related Supplier and Ingredient

            var inventory = await _context.Inventories
                .Include(i => i.Supplier)
                .Include(i => i.Ingredient)
                .ToListAsync();


            // Suppliers dropdown list

            ViewBag.Suppliers = await _context.Suppliers
                .ToListAsync();


            // Ingredients dropdown list

            ViewBag.Ingredients = await _context.Ingredients
                .ToListAsync();


            return View(inventory);
        }


        // =====================================================
        // POST: Inventory/Create
        // =====================================================

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


                // Reload existing inventory records

                var inventories = await _context.Inventories
                    .Include(i => i.Supplier)
                    .Include(i => i.Ingredient)
                    .ToListAsync();


                return View("Index", inventories);
            }


            // =================================================
            // Check Supplier
            // =================================================

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s =>
                    s.SupplierId == inventory.SupplierId);


            if (supplier == null)
            {
                return NotFound("Supplier was not found.");
            }


            // =================================================
            // Check Ingredient
            // =================================================

            var ingredient = await _context.Ingredients
                .FirstOrDefaultAsync(i =>
                    i.IngredientId == inventory.IngredientId);


            if (ingredient == null)
            {
                return NotFound("Ingredient was not found.");
            }


            // =================================================
            // Transaction
            // =================================================

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