using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Restaurant.Data;
using Online_Restaurant.Models;
using System.Diagnostics;

namespace Online_Restaurant.Controllers
{
    public class HomeController : Controller
    {
        //public IActionResult TestError()
        //{
        //    throw new Exception("This is a test exception");
        //}
        //[HttpGet]
        //public IActionResult TestJsonError()
        //{
        //    throw new Exception("This is a test JSON exception");
        //}
        private readonly AppdbContext _context;
        //private const int BestSellersCount = 10;
        private readonly IConfiguration _configuration;

        public HomeController(AppdbContext context,IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }
        public async Task<IActionResult> GetBestSellers()
        {
            int bestSellersCount =
                _configuration.GetValue<int>("BestSellersCount");

            var bestSellers = await _context.OrderDetails
                .GroupBy(od => od.Menu_ItemId)
                .Select(g => new
                {
                    MenuItemId = g.Key,
                    UnitsSold = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(g => g.UnitsSold)
                .Take(bestSellersCount)
                .Join(
                    _context.MenuItems,
                    g => g.MenuItemId,
                    m => m.MenuItemId,
                    (g, m) => m
                )
                .Where(m => m.Available)
                .ToListAsync();

            if (bestSellers.Count == 0)
            {
                bestSellers = await _context.MenuItems
                    .Where(m => m.Available)
                    .Take(bestSellersCount)
                    .ToListAsync();
            }

            return Json(bestSellers);
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