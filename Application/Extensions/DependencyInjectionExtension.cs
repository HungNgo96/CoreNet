using Application.Behaviors;
using FluentValidation;
using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjectionExtension).Assembly;

            services.AddMediatR(configuration =>
            {
                configuration.NotificationPublisher = new TaskWhenAllPublisher();
                configuration.RegisterServicesFromAssembly(assembly);
                //configuration.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
                //configuration.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
            });

            services.AddValidatorsFromAssembly(assembly);

            //_ = services.AddAutoMapper(assembly);

            return services;
        }
    }
}
