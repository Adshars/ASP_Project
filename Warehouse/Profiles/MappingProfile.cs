using AutoMapper;
using WarehouseAPI.DTO;
using WarehouseAPI.Model;
using WarehouseAPI.Data;
using Warehouse.DTO;

namespace WarehouseAPI.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDTO>().ReverseMap();
            CreateMap<Product, ProductCreateDTO>().ReverseMap();
            CreateMap<Category, CategoryDTO>().ReverseMap();
            CreateMap<Category, CategoryDetailsDTO>().ReverseMap();
        }
    }
}