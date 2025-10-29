using Microsoft.EntityFrameworkCore;

namespace SmartLibrary.Web.Core.Models
{
    [Index(nameof(Title), nameof(AuthorId), IsUnique = true)]
    public class Book : BaseModel
    {
        public int Id { get; set; }

        [MaxLength(500)]
        public string Title { get; set; } = null!;

        [MaxLength(200)]
        public string Publisher { get; set; } = null!;

        public DateTime PublishingDate { get; set; }

        public string? ImageUrl { get; set; }

        public string? ImageThumbnailUrl { get; set; }

        public string? ImagePublicId { get; set; }

   
        [MaxLength(50)]
        public string Hall { get; set; } = null!;

        public bool IsAvailableForRental { get; set; }

        public string Description { get; set; } = null!;

        public int AuthorId { get; set; }

        public virtual Author? Author { get; set; }

        public virtual ICollection<BookCategory> BookCategories { get; set; } = new HashSet<BookCategory>();
    }
}
