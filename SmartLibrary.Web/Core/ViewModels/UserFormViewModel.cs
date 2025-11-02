using Microsoft.AspNetCore.Mvc.Rendering;
using SmartLibrary.Web.Consts;

namespace SmartLibrary.Web.Core.ViewModels
{
    public class UserFormViewModel
    {
        public string? Id { get; set; }

        [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;


        [MaxLength(20, ErrorMessage = Errors.MaxLength)]
        public string UserName { get; set; } = null!;

        

        [MaxLength(200, ErrorMessage = Errors.MaxLength), EmailAddress]
        public string Email { get; set; } = null!;


        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 8)]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = Errors.ConfirmPasswordNotMatch)]
        public string ConfirmPassword { get; set; } = null!;


        [Display(Name = "Roles")]
        public virtual IList<string> SelectedRoles { get; set; } = new List<string>();

        public virtual IEnumerable<SelectListItem>? Roles { get; set; }
    }
}
