using AutoMapper;
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
        }
    }
}
