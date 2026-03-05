using AutoMapper;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.DTO;

namespace Mango.Services.ShoppingCartAPI
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<CartHeader, CartHeaderDTO>().ReverseMap();
            CreateMap<CartDetails, CartDetailsDTO>().ReverseMap();
        }
    }
}
