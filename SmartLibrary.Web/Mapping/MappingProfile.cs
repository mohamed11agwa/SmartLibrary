using AutoMapper;
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
        }
    }
}
