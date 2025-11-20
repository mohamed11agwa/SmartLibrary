using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartLibrary.Web.Consts;
using SmartLibrary.Web.Core.Enums;
using SmartLibrary.Web.Core.Models;
using SmartLibrary.Web.Core.Utilities;
using SmartLibrary.Web.Extensions;
using System.Net.Mime;

namespace SmartLibrary.Web.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReportsController(ApplicationDbContext context, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Books(IList<int> selectedAuthors, IList<int> selectedCategories,
            int? pageNumber)
        {
            var authors = _context.Authors.OrderBy(a => a.Name).ToList();
            var categories = _context.Categories.OrderBy(a => a.Name).ToList();

            IQueryable<Book> books = _context.Books
                        .Include(b => b.Author)
                        .Include(b => b.BookCategories)
                        .ThenInclude(c => c.Category)
                        .Where(b => (!selectedAuthors.Any() || selectedAuthors.Contains(b.AuthorId))
                        && (!selectedCategories.Any() || b.BookCategories.Any(c => selectedCategories.Contains(c.CategoryId))));

            //if (selectedAuthors.Any())
            //    books = books.Where(b => selectedAuthors.Contains(b.AuthorId));

            //if (selectedCategories.Any())
            //    books = books.Where(b => b.Categories.Any(c => selectedCategories.Contains(c.CategoryId)));

            var viewModel = new BooksReportViewModel
            {
                Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors),
                Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories)
            };

            if (pageNumber is not null)
                viewModel.Books = PaginatedList<Book>.Create(books, pageNumber ?? 0, (int)ReportsConfigurations.PageSize);

            return View(viewModel);
        }



        //public async Task<IActionResult> ExportBooksToExcel(string authors, string categories)
        //{
        //    var selectedAuthors = authors?.Split(',');
        //    var selectedCategories = categories?.Split(',');

        //    var books = _context.Books
        //                .Include(b => b.Author)
        //                .Include(b => b.Categories)
        //                .ThenInclude(c => c.Category)
        //                .Where(b => (string.IsNullOrEmpty(authors) || selectedAuthors!.Contains(b.AuthorId.ToString()))
        //                    && (string.IsNullOrEmpty(categories) || b.Categories.Any(c => selectedCategories!.Contains(c.CategoryId.ToString()))))
        //                .ToList();

        //    using var workbook = new XLWorkbook();

        //    var sheet = workbook.AddWorksheet("Books");

        //    sheet.AddLocalImage(_logoPath);

        //    var headerCells = new string[] { "Title", "Author", "Categories", "Publisher",
        //        "Publishing Date", "Hall", "Available for rental", "Status" };

        //    sheet.AddHeader(headerCells);

        //    for (int i = 0; i < books.Count; i++)
        //    {
        //        sheet.Cell(i + _sheetStartRow, 1).SetValue(books[i].Title);
        //        sheet.Cell(i + _sheetStartRow, 2).SetValue(books[i].Author!.Name);
        //        sheet.Cell(i + _sheetStartRow, 3).SetValue(string.Join(", ", books[i].Categories!.Select(c => c.Category!.Name)));
        //        sheet.Cell(i + _sheetStartRow, 4).SetValue(books[i].Publisher);
        //        sheet.Cell(i + _sheetStartRow, 5).SetValue(books[i].PublishingDate.ToString("d MMM, dddd"));
        //        sheet.Cell(i + _sheetStartRow, 6).SetValue(books[i].Hall);
        //        sheet.Cell(i + _sheetStartRow, 7).SetValue(books[i].IsAvailableForRental ? "Yes" : "No");
        //        sheet.Cell(i + _sheetStartRow, 8).SetValue(books[i].IsDeleted ? "Deleted" : "Available");
        //    }

        //    sheet.Format();
        //    sheet.AddTable(books.Count, headerCells.Length);
        //    sheet.ShowGridLines = false;

        //    await using var stream = new MemoryStream();

        //    workbook.SaveAs(stream);

        //    return File(stream.ToArray(), MediaTypeNames.Application.Octet, "Books.xlsx");
        //}

        //public async Task<IActionResult> ExportBooksToPDF(string authors, string categories)
        //{
        //    var selectedAuthors = authors?.Split(',');
        //    var selectedCategories = categories?.Split(',');

        //    var books = _context.Books
        //                .Include(b => b.Author)
        //                .Include(b => b.Categories)
        //                .ThenInclude(c => c.Category)
        //                .Where(b => (string.IsNullOrEmpty(authors) || selectedAuthors!.Contains(b.AuthorId.ToString()))
        //                    && (string.IsNullOrEmpty(categories) || b.Categories.Any(c => selectedCategories!.Contains(c.CategoryId.ToString()))))
        //                .ToList();

        //    //var html = await System.IO.File.ReadAllTextAsync($"{_webHost.WebRootPath}/templates/report.html");

        //    //html = html.Replace("[Title]", "Books");

        //    //var body = "<table><thead><tr><th>Title</th><th>Author</th></tr></thead><tbody>";

        //    //foreach (var book in books)
        //    //{
        //    //    body += $"<tr><td>{book.Title}</td><td>{book.Author!.Name}</td></tr>";
        //    //}

        //    //body += "</body></table>";

        //    //html = html.Replace("[body]", body);

        //    var viewModel = _mapper.Map<IEnumerable<BookViewModel>>(books);

        //    var templatePath = "~/Views/Reports/BooksTemplate.cshtml";
        //    var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templatePath, viewModel);

        //    var pdf = Pdf
        //        .From(html)
        //        .EncodedWith("Utf-8")
        //        .OfSize(PaperSize.A4)
        //        .WithMargins(1.Centimeters())
        //        .Landscape()
        //        .Content();

        //    return File(pdf.ToArray(), MediaTypeNames.Application.Octet, "Books.pdf");
        //}
    }
}
