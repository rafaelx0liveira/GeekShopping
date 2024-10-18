using AutoMapper;
using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Model;

namespace GeekShopping.CartAPI.Config
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                // Mapping ProductVO to Product
                config.CreateMap<ProductVO, Product>().ReverseMap(); // ReverseMap for two-way mapping

                // Mapping CartHeaderVO to CartHeader
                config.CreateMap<CartHeaderVO, CartHeader>().ReverseMap();

                // Mapping CartDetailVO to CartDetail
                config.CreateMap<CartDetailVO, CartDetail>().ReverseMap();

                // Mapping CartVO to Cart
                config.CreateMap<CartVO, Cart>().ReverseMap();

            });

            // Validate the mapping configuration
            mappingConfig.AssertConfigurationIsValid();

            return mappingConfig;
        }
    }
}
