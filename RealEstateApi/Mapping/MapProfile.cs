using AutoMapper;
using RealEstateApi.DTOs;
using RealEstateApi.Models;

namespace RealEstateApi.Mapping
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            CreateMap<Listing, ListingDto>()
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))  // Şehir ilişkisi
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address)); // Adres eşlemesi

            CreateMap<ListingDto, Listing>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.CityId, opt => opt.MapFrom(src => src.CityId));

            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<AppUser, UserDto>().ReverseMap();
            CreateMap<ListingCreateDto, Listing>();
            CreateMap<ListingImage, ListingImageDto>().ReverseMap();

            CreateMap<Comment, CommentDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));

            CreateMap<CommentDto, Comment>();
            CreateMap<Favorite, FavoriteDTO>().ReverseMap();
            CreateMap<City, CityDTO>().ReverseMap();
        }
    }
}
