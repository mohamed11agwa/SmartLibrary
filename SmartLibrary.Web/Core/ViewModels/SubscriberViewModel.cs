using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartLibrary.Web.Consts;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace SmartLibrary.Web.Core.ViewModels
{
    public class SubscriberViewModel
    {
        public int Id { get; set; }
        public string? key { get; set; }
        public string? FullName { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.Now;
        public string? NationalId { get; set; } = null!;
        public string? MobilePhone { get; set; } = null!;
        public bool? HasWhatsApp { get; set; }
        public string Email { get; set; } = null!;
        public string? Area { get; set; }
        public string? Governorate { get; set; }
        public string? Address { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public bool IsBlackListed { get; set; }
        public DateTime CreatedOn { get; set; }

        public IEnumerable<SubscriptionViewModel> Subscriptions { get; set; } = new List<SubscriptionViewModel>();
        public IEnumerable<RentalViewModel> Rentals { get; set; } = new List<RentalViewModel>();
    }
}
