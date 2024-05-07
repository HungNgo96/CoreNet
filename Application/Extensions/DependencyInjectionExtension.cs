using System.Reflection;
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
            _ = services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddMediatR(configuration =>
            {
                configuration.NotificationPublisher = new TaskWhenAllPublisher();
                configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                //configuration.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
                configuration.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
            });

            //_ = services.AddAutoMapper(assembly);

            return services;
        }
    }
}
