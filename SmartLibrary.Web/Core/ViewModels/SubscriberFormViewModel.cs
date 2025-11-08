using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartLibrary.Web.Consts;
using SmartLibrary.Web.Core.Models;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace SmartLibrary.Web.Core.ViewModels
{
    public class SubscriberFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(100)]
        [Display(Name = "First Name")]
        [RegularExpression(RegexPatterns.DenySpecialCharacters, ErrorMessage = Errors.DenySpecialCharacters)]
        public string FirstName { get; set; } = null!;

        [MaxLength(100)]
        [Display(Name = "Last Name")]
        [RegularExpression(RegexPatterns.DenySpecialCharacters, ErrorMessage = Errors.DenySpecialCharacters)]
        public string LastName { get; set; } = null!;

        [Display(Name = "Date of Birth")]
        [AssertThat("DateOfBirth <= Today()", ErrorMessage = Errors.NotAllowFuture)]
        public DateTime DateOfBirth { get; set; } = DateTime.Now;

        [MaxLength(14)]
        [Display(Name = "National ID")]
        [Remote("AllowNationalId", null!, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        [RegularExpression(RegexPatterns.NationalId, ErrorMessage = Errors.InvalidNationalId)]
        public string NationalId { get; set; } = null!;

        [MaxLength(11)]
        [Display(Name = "Mobile Phone")]
        [Remote("AllowMobileNumber", null!, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        [RegularExpression(RegexPatterns.MobileNumber, ErrorMessage = Errors.InvalidMobileNumber)]
        public string MobilePhone { get; set; } = null!;

        public bool HasWhatsApp { get; set; }

        [MaxLength(150), EmailAddress]
        [Remote("AllowEmail", null!, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string Email { get; set; } = null!;

        [RequiredIf("Id == 0", ErrorMessage = Errors.EmptyImage)]
        public IFormFile? Image { get; set; }


        [Display(Name = "Area")]
        public int AreaId { get; set; }

        public IEnumerable<SelectListItem>? Areas { get; set; } = new List<SelectListItem>();

        [Display(Name = "Governorate")]
        public int GovernorateId { get; set; }

        public IEnumerable<SelectListItem>? Governorates { get; set; }

        [MaxLength(500)]
        public string Address { get; set; } = null!;


        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }


    }
}
