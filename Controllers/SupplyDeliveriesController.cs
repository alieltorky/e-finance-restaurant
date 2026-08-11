using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;

namespace Online_Restaurant.Controllers
{
    public class SupplyDeliveriesController : Controller
    {
        private readonly AppdbContext _context;

        public SupplyDeliveriesController(AppdbContext context)
        {
            _context = context;
        }


        // =====================================================
        // GET: SupplyDeliveries
        // =====================================================

        public async Task<IActionResult> Index()
        {
            // Get all supply deliveries
            // and load their related Supplier and Ingredient

            var deliveries = await _context.SupplyDeliveries
                .Include(d => d.Supplier)
                .Include(d => d.Ingredient)
                .ToListAsync();


            // Suppliers dropdown list

            ViewBag.Suppliers = await _context.Suppliers
                .ToListAsync();


            // Ingredients dropdown list

            ViewBag.Ingredients = await _context.Ingredients
                .ToListAsync();


            return View(deliveries);
        }


        // =====================================================
        // POST: SupplyDeliveries/Create
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            SupplyDelivery supplyDelivery)
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


                // Reload existing deliveries

                var deliveries = await _context.SupplyDeliveries
                    .Include(d => d.Supplier)
                    .Include(d => d.Ingredient)
                    .ToListAsync();


                return View("Index", deliveries);
            }


            // Check that the selected Supplier exists

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s =>
                    s.SupplierId == supplyDelivery.SupplierId);


            if (supplier == null)
            {
                return NotFound("Supplier was not found.");
            }


            // Check that the selected Ingredient exists

            var ingredient = await _context.Ingredients
                .FirstOrDefaultAsync(i =>
                    i.IngredientId == supplyDelivery.IngredientId);


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
                        // Add the new supply delivery

                        _context.SupplyDeliveries
                            .Add(supplyDelivery);


                        // Save the new delivery

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
                    "Error happened! Supply order could not be completed."
                );
            }
        }
    }
}