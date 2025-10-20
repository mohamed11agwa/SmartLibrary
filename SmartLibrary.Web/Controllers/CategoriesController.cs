using Microsoft.AspNetCore.Mvc;

namespace SmartLibrary.Web.Controllers
{
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
