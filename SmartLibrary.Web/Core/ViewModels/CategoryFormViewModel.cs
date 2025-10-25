using Microsoft.AspNetCore.Mvc;

namespace SmartLibrary.Web.Core.ViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }
        [MaxLength(100, ErrorMessage ="Maximum Length Can't Be more than 100 Characters")]
        [Remote("AllowItem", null, AdditionalFields ="Id" , ErrorMessage = "Category with the same name is already exists!")]
        public string Name { get; set; } = null!;

    }
}
