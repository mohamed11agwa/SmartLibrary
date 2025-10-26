
using Microsoft.EntityFrameworkCore;

namespace SmartLibrary.Web.Core.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Category: BaseModel
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public virtual ICollection<BookCategory> BookCategories { get; set; } = new HashSet<BookCategory>();

    }
}
