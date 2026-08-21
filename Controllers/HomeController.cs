using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;
using System.Diagnostics;

namespace Online_Restaurant.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppdbContext _context;
        private const int BestSellersCount = 10;

        public HomeController(AppdbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var bestSellers = await _context.OrderDetails
                .GroupBy(od => od.Menu_ItemId)
                .Select(g => new
                {
                    MenuItemId = g.Key,
                    UnitsSold = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(g => g.UnitsSold)
                .Take(BestSellersCount)
                .Join(_context.MenuItems,   
                    g => g.MenuItemId,   //g for the previous group by + select
                    m => m.MenuItemId,  //m for the menuitems table
                    (g, m) => m)
                .Where(m => m.Available)
                .ToListAsync();

            if (bestSellers.Count == 0)
            {
                bestSellers = await _context.MenuItems
                    .Where(m => m.Available)
                    .Take(BestSellersCount)
                    .ToListAsync();
            }

            return View(bestSellers);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}