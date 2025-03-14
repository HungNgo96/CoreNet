using Application.Responses.Products;
using Domain.Entities.Products;
using Mapster;

namespace Application.DependencyInjections.Configurations
{
    public static class MapsterConfiguration
    {
        public static void RegisterMappings()
        {
            var config = TypeAdapterConfig.GlobalSettings;

            config.NewConfig<Product, GetProductResponse>();
        }
    }
}
