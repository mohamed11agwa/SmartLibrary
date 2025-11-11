using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartLibrary.Web.Core.Models;

namespace SmartLibrary.Web.Mapping
{

    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            //Category
            CreateMap<Category, CategoryViewModel>();
            CreateMap<CategoryFormViewModel, Category>().ReverseMap();
            CreateMap<Category, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));
                



            //Author
            CreateMap<Author, AuthorViewModel>();
            CreateMap<AuthorFormViewModel, Author>().ReverseMap();
            CreateMap<Author, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));


            //Book
            CreateMap<BookFormViewModel, Book>().ReverseMap()
                .ForMember(dest => dest.Categories, opt => opt.Ignore());

            CreateMap<Book, BookViewModel>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author!.Name))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.BookCategories.Select(c => c.Category!.Name).ToList()))
                .ForMember(dest => dest.BookCopies, opt => opt.MapFrom(src => src.Copies));


            //BookCopy
            CreateMap<BookCopy, BookCopyViewModel>()
                .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book!.Title));

            CreateMap<BookCopy, BookCopyFormViewModel>().ReverseMap();



            //User
            CreateMap<ApplicationUser, UserViewModel>();

            CreateMap<ApplicationUser, UserFormViewModel>();
            CreateMap<UserFormViewModel, ApplicationUser>()
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.UserName.ToUpper()));



            //Governorates & Areas
            CreateMap<Governorate, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));
            CreateMap<Area, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            //Subscriber
            CreateMap<Subscriber, SubscriberFormViewModel>().ReverseMap();
            CreateMap<Subscriber, SubscriberSearchResultViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            CreateMap<Subscriber, SubscriberViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area!.Name))
                .ForMember(dest => dest.Governorate, opt => opt.MapFrom(src => src.Governorate!.Name))
                .ForMember(dest => dest.IsBlackListed, opt => opt.MapFrom(src => src.IsBlackList));


            CreateMap<Subscription, SubscriptionViewModel>();
            CreateMap<RentalCopy, RentalCopyViewModel>();
        }
    }
}
