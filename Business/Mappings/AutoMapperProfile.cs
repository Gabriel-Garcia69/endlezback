using AutoMapper;
using Core.Dtos.Category;
using Core.Dtos.CustomerAddress;
using Core.Dtos.Order;
using Core.Dtos.OrderStatus;
using Core.Dtos.OrderType;
using Core.Dtos.Product;
using Core.Dtos.Profile;
using Core.Dtos.User;
using Core.Entities;

namespace Business.Mappings
{
    public class AutoMapperProfile: AutoMapper.Profile
    {
        public AutoMapperProfile()
        {

            CreateMap<Category, CategoryDto>();
            CreateMap<CategoryCreateDto, Category>();
            CreateMap<CategoryUpdateDto, Category>();


         
            CreateMap<Core.Entities.Profile, ProfileDto>();
            CreateMap<ProfileDto, Core.Entities.Profile>();
            CreateMap<ProfileCreateDto, Core.Entities.Profile>();
            CreateMap<ProfileUpdateDto, Core.Entities.Profile>();

            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<UserUpdateDto, User>();
            CreateMap<User, UserDto>()
                 .ForMember(dest => dest.ProfileName, opt => opt.MapFrom(src => src.Profile!.Title))
                 .ReverseMap();


            CreateMap<UserDto, User>();

            CreateMap<CustomerAddressCreateDto, CustomerAddress>();
            CreateMap<CustomerAddressUpdateDto, CustomerAddress>();
            CreateMap<CustomerAddress, CustomerAddressDto>();

            CreateMap<OrderType, OrderTypeDto>();
            CreateMap<OrderTypeCreateDto, OrderType>();
            CreateMap<OrderTypeUpdateDto, OrderType>();

            CreateMap<OrderStatus, OrderStatusDto>();
            CreateMap<OrderStatus, OrderStatusUpdateDto>();
            CreateMap<OrderStatusCreateDto, OrderStatus>();



            CreateMap<OrderCreateDto, Order>()
                      .ForMember(dest => dest.Id, opt => opt.Ignore()) // Ignorar Id, ya que será generado automáticamente
                      .ForMember(dest => dest.OrderProducts, opt => opt.MapFrom(src => src.Products));

            // Mapping de OrderDto a Order
            CreateMap<OrderDto, Order>();

            // Mapping de OrderProductCreateDto a OrderProduct
            CreateMap<OrderProductCreateDto, OrderProduct>()
                .ForMember(dest => dest.OrderId, opt => opt.Ignore()); // Ignorar OrderId, se establecerá más tarde

            // Mapping de OrderUpdateDto a Order (solo campos editables, ignora navegación)
            CreateMap<OrderUpdateDto, Order>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.OrderType, opt => opt.Ignore())
                .ForMember(dest => dest.OrderStatus, opt => opt.Ignore())
                .ForMember(dest => dest.OrderProducts, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());

            // Mapping de Order a OrderDto
            CreateMap<Order, OrderDto>();

            // Mapping de OrderProduct a OrderProductDto (incluye nombre del producto)
            CreateMap<OrderProduct, OrderProductDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Title : string.Empty));

            // Mapping de OrderProduct a OrderProductCreateDto
            CreateMap<OrderProduct, OrderProductCreateDto>();

            // Mapeo de Product a ProductDto
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Title));

            // Mapeo de ProductCreateDto a Product
            CreateMap<ProductCreateDto, Product>();

            // Mapeo de ProductUpdateDto a Product
            CreateMap<ProductUpdateDto, Product>();
                //.ForMember(dest => dest.UserChangedId, opt => opt.MapFrom(src => DateTime.Now));

        }
    }
}
