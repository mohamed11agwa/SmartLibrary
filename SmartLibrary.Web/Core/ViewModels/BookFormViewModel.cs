using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartLibrary.Web.Consts;
using SmartLibrary.Web.Core.Models;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace SmartLibrary.Web.Core.ViewModels
{
    public class BookFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(500, ErrorMessage = Errors.MaxLength)]
        [Remote("AllowItem", null!, AdditionalFields = "Id, AuthorId", ErrorMessage = Errors.DuplicatedBook)]
        public string Title { get; set; } = null!;


        [MaxLength(200, ErrorMessage = Errors.MaxLength)]
        public string Publisher { get; set; } = null!;


        [Display(Name = "Publishing Date")]
        [AssertThat("PublishingDate <= Today()", ErrorMessage =Errors.NotAllowFuture)]
        public DateTime PublishingDate { get; set; } = DateTime.Now;

        public IFormFile? Image { get; set; }

        public string? ImageUrl { get; set; }

        public string? ImageThumbnailUrl { get; set; }

        [MaxLength(50, ErrorMessage = Errors.MaxLength)]
        public string Hall { get; set; } = null!;


        [Display(Name ="Is available For Rental?")]
        public bool IsAvailableForRental { get; set; }

        public string Description { get; set; } = null!;


        [Display(Name ="Author")]
        [Remote("AllowItem", null!, AdditionalFields = "Id, Title", ErrorMessage = Errors.DuplicatedBook)]
        public int AuthorId { get; set; }

        public virtual IEnumerable<SelectListItem>? Authors { get; set; }

        [Display(Name ="Categories")]
        public virtual IList<int> SelectedCategories { get; set; } = new List<int>();

        public virtual IEnumerable<SelectListItem>? Categories { get; set; }


    }
}
