using Microsoft.Extensions.DependencyInjection;

namespace Presentation.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            return services;
        }
    }
}
