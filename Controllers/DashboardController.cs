using Microsoft.AspNetCore.Mvc;

namespace Online_Restaurant.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
