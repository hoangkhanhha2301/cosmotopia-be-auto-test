using AutoMapper;
using Cosmetics.DTO.Affiliate;
using Cosmetics.DTO.Brand;
using Cosmetics.DTO.Cart;
using Cosmetics.DTO.Category;
using Cosmetics.DTO.Order;
using Cosmetics.DTO.OrderDetail;
using Cosmetics.DTO.Payment;
using Cosmetics.DTO.Product;
using Cosmetics.DTO.User;
using Cosmetics.DTO.User.Admin;
using Cosmetics.Models;

namespace Cosmetics.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<User, UserAdminDTO>().ReverseMap();
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
            //Affiliate


            //Payment
            CreateMap<PaymentTransaction, PaymentTransactionDTO>().ReverseMap();
            CreateMap<PaymentTransaction, PaymentRequestDTO>().ReverseMap();
            CreateMap<PaymentTransaction, PaymentResponseDTO>().ReverseMap();


            // Affiliate
            CreateMap<AffiliateProfile, AffiliateProfileDto>().ReverseMap();
            CreateMap<AffiliateProductLink, AffiliateLinkDto>().ReverseMap();
            CreateMap<TransactionAffiliate, WithdrawalResponseDto>()
                .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.TransactionAffiliatesId))
                .ReverseMap();
=======
            CreateMap<AffiliateProfile, AffiliateIncomeDto>().ReverseMap();
            CreateMap<AffiliateProfile, GenerateAffiliateLinkDto>().ReverseMap();
            CreateMap<AffiliateProfile, RegisterAffiliateDto>().ReverseMap();
            CreateMap<AffiliateProfile, TopProductDto>().ReverseMap();
            //Cart
            CreateMap<CartDetail, CartDetailDTO>().ReverseMap();
            CreateMap<CartDetail, CartDetailInputDTO>().ReverseMap();


        }
    }
}
