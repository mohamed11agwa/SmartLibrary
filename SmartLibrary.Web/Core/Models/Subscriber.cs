using Microsoft.EntityFrameworkCore;

namespace SmartLibrary.Web.Core.Models
{
    [Index(nameof(NationalId), IsUnique = true)]
    [Index(nameof(MobilePhone), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class Subscriber:BaseModel
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [MaxLength(100)]
        public string LastName { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }

        [MaxLength(20)]
        public string NationalId { get; set; } = null!;

        [MaxLength(15)]
        public string MobilePhone { get; set; } = null!;

        public bool HasWhatsApp { get; set; }

        [MaxLength(150)]
        public string Email { get; set; } = null!;

        [MaxLength(500)]
        public string ImageUrl { get; set; } = null!;

        [MaxLength(500)]
        public string ImageThumbnailUrl { get; set; } = null!;

        public int AreaId { get; set; }

        public virtual Area? Area { get; set; }

        public int GovernorateId { get; set; }

        public virtual Governorate? Governorate { get; set; }

        [MaxLength(500)]
        public string Address { get; set; } = null!;

        public bool IsBlackList { get; set; }


    }
}
