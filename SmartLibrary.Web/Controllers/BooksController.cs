using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using SmartLibrary.Web.Consts;
using SmartLibrary.Web.Core.Models;
using SmartLibrary.Web.Settings;
using System.Threading.Tasks;

namespace SmartLibrary.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private List<string> _allowedExtensions = new List<string>() { ".jpg", ".jpeg", ".png"};
        private int _maxAllowedSize = 2097152;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly Cloudinary _cloudinary;
        public BooksController(ApplicationDbContext context, IMapper mapper, IWebHostEnvironment webHostEnvironment,
                                                                            IOptions<CloudinarySettings> cloudinary)
        {
            _context = context;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            Account account = new Account()
            {
                Cloud = cloudinary.Value.Cloud,
                ApiKey = cloudinary.Value.ApiKey,
                ApiSecret = cloudinary.Value.ApiSecret
            };
            _cloudinary = new Cloudinary(account);
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = PopulateViewModel();
            return View("Form", viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = PopulateViewModel(model);
                return View("Form", model);
            }
            var book = _mapper.Map<Book>(model);
            if(model.Image is not null)
            {
                var extension = Path.GetExtension(model.Image.FileName);
                if (!_allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.NotAllowedExtensions);
                    return View("Form", PopulateViewModel(model));
                }
                if (model.Image.Length > _maxAllowedSize)
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.MaxSize);
                    return View("Form", PopulateViewModel(model));
                }
                var imageName = $"{Guid.NewGuid()}{extension}";
                //var path = Path.Combine($"{_webHostEnvironment.WebRootPath}/images/books", imageName);
                //using var stream = System.IO.File.Create(path);
                //await model.Image.CopyToAsync(stream);
                //book.ImageUrl = imageName;


                //Host Image to Cloudinary
                using var stream = model.Image.OpenReadStream();
                var imageParams = new ImageUploadParams()
                {
                    File = new FileDescription(imageName, stream),
                    UseFilename = true,
                };
                var result = await _cloudinary.UploadAsync(imageParams);

                book.ImageUrl = result.SecureUrl.ToString();
                book.ImageThumbnailUrl = GetThumbnailUrl(book.ImageUrl);
                book.ImagePublicId = result.PublicId;
            }
            foreach (var category in model.SelectedCategories)
            {
                book.BookCategories.Add(new BookCategory { CategoryId = category });
            }
            _context.Add(book);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var book = _context.Books.Include(b => b.BookCategories).SingleOrDefault(b => b.Id == id);
            if(book is null)
                return NotFound();
            var model = _mapper.Map<BookFormViewModel>(book);
            var viewModel = PopulateViewModel(model);

            viewModel.SelectedCategories =  book.BookCategories.Select(c => c.CategoryId).ToList();
            return View("Form",viewModel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = PopulateViewModel(model);
                return View("Form", model);
            }
            var book = _context.Books.Include(b => b.BookCategories).SingleOrDefault(b=> b.Id == model.Id);
            if (book is null)
                return NotFound();

            string ImagePublicId = null;
            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(book.ImageUrl))
                {
                    //var oldImagePath = Path.Combine($"{_webHostEnvironment.WebRootPath}/images/books", book.ImageUrl);
                    //if(System.IO.File.Exists(oldImagePath))
                    //    System.IO.File.Delete(oldImagePath);

                    await _cloudinary.DeleteResourcesAsync(book.ImagePublicId);
                }
                var extension = Path.GetExtension(model.Image.FileName);
                if (!_allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.NotAllowedExtensions);
                    return View("Form", PopulateViewModel(model));
                }
                if (model.Image.Length > _maxAllowedSize)
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.MaxSize);
                    return View("Form", PopulateViewModel(model));
                }
                var imageName = $"{Guid.NewGuid()}{extension}";
                //var path = Path.Combine($"{_webHostEnvironment.WebRootPath}/images/books", imageName);
                //using var stream = System.IO.File.Create(path);
                //await model.Image.CopyToAsync(stream);
                //model.ImageUrl = imageName;

                using var stream = model.Image.OpenReadStream();
                var imageParams = new ImageUploadParams()
                {
                    File = new FileDescription(imageName, stream),
                    UseFilename = true,
                };
                var result = await _cloudinary.UploadAsync(imageParams);

                model.ImageUrl = result.SecureUrl.ToString();
                ImagePublicId = result.PublicId;


            }
            else if((model.Image is null) && !string.IsNullOrEmpty(book.ImageUrl))
               model.ImageUrl = book.ImageUrl;

            book = _mapper.Map(model, book);
            book.LastUpdatedOn = DateTime.Now;

            book.ImageThumbnailUrl = GetThumbnailUrl(book.ImageUrl!);
            book.ImagePublicId = ImagePublicId;

            book.BookCategories.Clear();
            foreach (var category in model.SelectedCategories)
            {
                book.BookCategories.Add(new BookCategory { CategoryId = category });
            }
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }



        public IActionResult AllowItem(BookFormViewModel model)
        {
            var book = _context.Books.SingleOrDefault(b => b.Title == model.Title && b.AuthorId == model.AuthorId);
            var isAllowed = book is null || book.Id.Equals(model.Id);
            return Json(isAllowed);
        }

        public string GetThumbnailUrl(string url)
        {
            var separator = "image/upload/";
            var urlParts = url.Split(separator);

            var thumbnail = $"{urlParts[0]}{separator}c_thumb,g_face,w_150/{urlParts[1]}";

            return thumbnail;
            
        }

        private BookFormViewModel PopulateViewModel(BookFormViewModel? model = null)
        {
            BookFormViewModel viewModel = model is null ? new BookFormViewModel() : model;
            var authors = _context.Authors.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();
            var categories = _context.Categories.Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToList();

            viewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
            viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);
            return viewModel;
        }
    }
}
