using Microsoft.AspNetCore.Mvc;

namespace SmartLibrary.Web.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        //[HttpGet]
        //public IActionResult Index()
        //{
            
        //}
    }
}
