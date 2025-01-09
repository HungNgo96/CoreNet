using System.Reflection;
using Application.Behaviors;
using Application.UseCases.v1.Products.Commands.CreateProduct;
using Contract.Abstractions.EventBus;
using FluentValidation;
using Infrastructure.BackgroundJobs;
using Infrastructure.Extensions;
using Infrastructure.MessageBroker;
using MassTransit;
using MediatR.NotificationPublishers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.DependencyInjections.Extensions;
using Quartz;

namespace Application.DependencyInjections.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddConfigQuartz(this IServiceCollection services)
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
            var assembly = typeof(DependencyInjectionExtension).Assembly;
            _ = services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddMediatR(configuration =>
            {
                configuration.NotificationPublisher = new TaskWhenAllPublisher();
                configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                //configuration.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
                configuration.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
                configuration.AddOpenBehavior(typeof(IdempotentCommandPipelineBehavior<,>));
            });

            //_ = services.AddAutoMapper(assembly);

            return services;
        }

        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConfigDbContext(configuration);

            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddConfigQuartz()
                .AddInfasOpenTelemetry(builder);

            return services.AddConfigureMassTransit();
        }

        private static IServiceCollection AddConfigureMassTransit(this IServiceCollection services)
        {
            _ = services.AddMassTransit((busConfigurator) =>
            {
                busConfigurator.SetKebabCaseEndpointNameFormatter();

                //busConfigurator.AddConsumer<ProductCreatedEventConsumer, ProductCreatedEventConsumerDefinition>();
                busConfigurator.AddConsumer<ProductCreatedEventConsumer>();
                //busConfigurator.AddConsumers(Assembly.GetExecutingAssembly());
                busConfigurator.UsingRabbitMq((context, configurator) =>
                {
                    configurator.PrefetchCount = 7;//to consumer in order but this is not performance
                    configurator.Host(new Uri("amqp://guest:guest@rabbitmq:5672"), (h) =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    configurator.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(5)));

                    //configurator.ReceiveEndpoint("product-service", e =>
                    //{
                    //    e.ConcurrentMessageLimit = 28; // only applies to this endpoint
                    //    e.PrefetchCount = 5;
                    //    e.ConfigureConsumer<ProductCreatedEventConsumer>(context);
                    //});
                    //configurator.Message<ProductCreatedEvent>(x =>
                    //{
                    //    x.SetEntityName("product-created-event-exchange-2");
                    //});
                    configurator.ConfigureEndpoints(context);///add attribute ExcludeFromConfigureEndpoints to ignore config
                });

                //services.AddOptions<MassTransitHostOptions>()
                //       .Configure(options =>
                //       {
                //           options.WaitUntilStarted = true;
                //           options.StartTimeout = TimeSpan.FromSeconds(30);
                //           options.StopTimeout = TimeSpan.FromSeconds(60);
                //       });
                //services.AddOptions<HostOptions>()
                //    .Configure(options =>
                //    {
                //        options.StartupTimeout = TimeSpan.FromSeconds(60);
                //        options.ShutdownTimeout = TimeSpan.FromSeconds(60);
                //    });
            });

            return services;
        }
    }
}
