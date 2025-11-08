using Microsoft.EntityFrameworkCore;

namespace SmartLibrary.Web.Core.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Governorate:BaseModel
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public virtual ICollection<Area> Areas { get; set; } = new List<Area>();

    }
}
