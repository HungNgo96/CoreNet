using Application.Services;
using Infrastructure.Persistence.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services
                .AddTransientServices()
                .AddScopeServices();

            return services;
        }

        public static IServiceCollection AddTransientServices(this IServiceCollection services)
        {
            var managers = typeof(ITransientService);

            var types = managers
                .Assembly
                .GetExportedTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(t => new
                {
                    Service = t.GetInterface($"I{t.Name}"),
                    Implementation = t
                })
                .Where(t => t.Service != null);
            foreach (var type in from type in types
                                 where managers.IsAssignableFrom(type.Service)
                                 select type)
            {
                services.AddTransient(type.Service, type.Implementation);
            }

            return services;
        }

        public static IServiceCollection AddScopeServices(this IServiceCollection services)
        {
            var managers = typeof(IScopeService);

            var types = managers
                .Assembly
                .GetExportedTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(t => new
                {
                    Service = t.GetInterface($"I{t.Name}"),
                    Implementation = t
                })
                .Where(t => t.Service != null);

            foreach (var type in types.Where(type => managers.IsAssignableFrom(type.Implementation)))
            {
                services.AddScoped(type.Service!, type.Implementation);
            }

            return services;
        }
    }
}
