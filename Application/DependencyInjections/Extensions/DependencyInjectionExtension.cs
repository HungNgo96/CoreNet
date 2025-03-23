using System.Reflection;
using Application.BackgroundJobs;
using Application.Behaviors;
using Application.DependencyInjections.Configurations;
using FluentValidation;
using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Application.DependencyInjections.Extensions
{
    public static class DependencyInjectionExtension
    {
        private static IServiceCollection AddConfigQuartz(this IServiceCollection services)
        {
            services.AddQuartz(config =>
            {
                var jobKey = new JobKey(nameof(ProcessOutboxMessageJob));

                config.
                AddJob<ProcessOutboxMessageJob>(jobKey)
                .AddTrigger(trigger =>
                {
                    trigger.ForJob(jobKey)
                    .WithSimpleSchedule(schedule =>
                    {
                        schedule.WithIntervalInSeconds(60).RepeatForever();
                    });
                });
            });

            services.AddQuartzHostedService();
            return services;
        }

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            //var assembly = typeof(DependencyInjectionExtension).Assembly;
            _ = services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddMediatR(configuration =>
            {
                configuration.NotificationPublisher = new TaskWhenAllPublisher();
                configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                //configuration.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
                configuration.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
                configuration.AddOpenBehavior(typeof(IdempotentCommandPipelineBehavior<,>));
            }).AddConfigQuartz();

            MapsterConfiguration.RegisterMappings();

            return services;
        }
    }
}
