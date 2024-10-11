﻿using AutoMapper;
using GeekShopping.ProductAPI.Data.ValueObjects;
using GeekShopping.ProductAPI.Model;

namespace GeekShopping.ProductAPI.Config
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                // Mapeamento de ProductVO para Product
                config.CreateMap<ProductVO, Product>();

                // Mapeamento de Product para ProductVO
                config.CreateMap<Product, ProductVO>();
            });

            // Valida a configuração do AutoMapper
            mappingConfig.AssertConfigurationIsValid();

            return mappingConfig;
        }
    }
}
