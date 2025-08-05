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
        CreateMap<ProductReadModel, ProductDto>()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags ?? new List<string>()))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));
        CreateMap<CategoryReadModel, CategoryDto>()
            .ForMember(dest => dest.Level, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.IsRoot, opt => opt.MapFrom(src => src.ParentCategoryId == null))
            .ForMember(dest => dest.HasChildren, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => (IEnumerable<CategoryDto>?)null))
            .ForMember(dest => dest.ParentCategoryId, opt => opt.MapFrom(src => src.ParentCategoryId));
        
        // Domain to DTO mappings
        CreateMap<ECommerce.Domain.Aggregates.ProductAggregate.Category, CategoryDto>()
            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children));
        
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