using AutoMapper;
using Cosmetics.DTO.Brand;
using Cosmetics.DTO.Category;
using Cosmetics.DTO.Order;
using Cosmetics.DTO.OrderDetail;
using Cosmetics.DTO.Product;
using Cosmetics.DTO.User;
using Cosmetics.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Cosmetics.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<Product, ProductDTO>().ReverseMap();  
            CreateMap<Category, CategoryDTO>().ReverseMap();
            CreateMap<Brand, BrandDTO>().ReverseMap();

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
