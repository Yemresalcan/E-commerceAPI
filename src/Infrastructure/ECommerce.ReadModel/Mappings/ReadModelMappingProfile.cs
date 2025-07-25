using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Queries.Products;
using ECommerce.ReadModel.Models;
using ECommerce.ReadModel.Services;

namespace ECommerce.ReadModel.Mappings;

/// <summary>
/// AutoMapper profile for mapping read models to DTOs
/// </summary>
public class ReadModelMappingProfile : Profile
{
    public ReadModelMappingProfile()
    {
        // Product mappings
        CreateMap<ProductReadModel, ProductDto>();
        CreateMap<CategoryReadModel, CategoryDto>();
        
        // Order mappings
        CreateMap<OrderReadModel, OrderDto>();
        CreateMap<OrderItemReadModel, OrderItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
            .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => 0m)); // Default discount
        CreateMap<PaymentReadModel, PaymentDto>();
        CreateMap<CustomerSummaryReadModel, CustomerSummaryDto>();
        
        // Customer mappings
        CreateMap<CustomerReadModel, CustomerDto>();
        CreateMap<AddressReadModel, AddressDto>();
        CreateMap<ProfileReadModel, ProfileDto>();
        CreateMap<CustomerStatisticsReadModel, CustomerStatisticsDto>();
        
        // Search result mappings
        CreateMap<ProductSearchResult, ProductSearchResultDto>()
            .ForMember(dest => dest.TotalPages, opt => opt.MapFrom(src => src.TotalPages));
        CreateMap<ProductSearchFacets, ProductSearchFacetsDto>();
    }
}