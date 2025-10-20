namespace SmartLibrary.Web.Core.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        [MaxLength(100, ErrorMessage ="Maximum Length Can't Be more than 100 Characters")]
        public string Name { get; set; } = null!;

    }
}
