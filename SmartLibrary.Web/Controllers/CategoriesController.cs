using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLibrary.Web.Core.Models;
using SmartLibrary.Web.Filters;

namespace SmartLibrary.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            //TODO: use ViewModel
            var categories = _context.Categories.AsNoTracking().ToList();
            return View(categories);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var category = new Category()
            {
                Name = model.Name
            };
            _context.Add(category);
            _context.SaveChanges();
            //Send Alerts Controllers to Views.
            //TempData["Message"] = "Saved Successfully";
            //return RedirectToAction(nameof(Index));
            return PartialView("_CategoryRow", category);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);
            if (category is null)
                return NotFound();
            CategoryViewModel viewModel = new CategoryViewModel()
            {
                Id = id,
                Name = category.Name
                
            };
            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
                BadRequest();
            var category = _context.Categories.Find(model.Id);
            if (category is null)
                return NotFound();
            category.Name = model.Name;
            category.LastUpdatedOn = DateTime.Now;
            _context.SaveChanges();
            return PartialView("_CategoryRow", category);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var category = _context.Categories.Find(id);
            if (category is null)
                return NotFound();
            category.IsDeleted = !category.IsDeleted;
            category.LastUpdatedOn = DateTime.Now;
            _context.SaveChanges();
            return Ok(category.LastUpdatedOn.ToString());
        }
    }
}
