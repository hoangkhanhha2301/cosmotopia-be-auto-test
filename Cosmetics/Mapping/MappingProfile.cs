using AutoMapper;
using Cosmetics.DTO.Brand;
using Cosmetics.DTO.Category;
using Cosmetics.DTO.Order;
using Cosmetics.DTO.OrderDetail;
using Cosmetics.DTO.Product;
using Cosmetics.DTO.User;
using Cosmetics.Models;

namespace Cosmetics.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDTO>().ReverseMap();
            //Product
            CreateMap<Product, ProductDTO>().ReverseMap();  
            CreateMap<Product, ProductCreateDTO>().ReverseMap();
            CreateMap<Product, ProductUpdateDTO>().ReverseMap();
            //Category
            CreateMap<Category, CategoryDTO>().ReverseMap();
            CreateMap<Category, CategoryCreateDTO>().ReverseMap();
            CreateMap<Category, CategoryUpdateDTO>().ReverseMap();
            //Brand
            CreateMap<Brand, BrandDTO>().ReverseMap();
            CreateMap<Brand, BrandCreateDTO>().ReverseMap();
            CreateMap<Brand, BrandUpdateDTO>().ReverseMap();
            //Order
            CreateMap<Order, OrderCreateDTO>().ReverseMap();
            CreateMap<Order, OrderResponseDTO>().ReverseMap();
            CreateMap<Order, OrderUpdateDTO>().ReverseMap();
            //OrderDetail
            CreateMap<OrderDetail, OrderDetailCreateDTO>().ReverseMap();
            CreateMap<OrderDetail, OrderDetailDTO>().ReverseMap();
            CreateMap<OrderDetail, OrderDetailUpdateDTO>().ReverseMap();
        }
    }
}
