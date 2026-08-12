using Microsoft.AspNetCore.Mvc;

namespace Online_Restaurant.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
