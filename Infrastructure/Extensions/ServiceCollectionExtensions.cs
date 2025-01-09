// HungNgo96

using Contract.Abstractions.EventBus;
using Domain.Core.AppSettings;
using Domain.Core.Extensions;
using Infrastructure.MessageBroker;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services
                .AddCacheService(builder.Configuration)
                .AddInfasOpenTelemetry(builder)
                .AddConfigureMassTransit();

            return services;
        }

        private static IServiceCollection AddCacheService(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetOptions<ConnectionOptions>() ?? new();

            if (options.CacheConnectionInMemory())
            {
                //services.AddMemoryCacheService();
                services.AddMemoryCache(memoryOptions => memoryOptions.TrackStatistics = true);
            }
            else
            {
                //services.AddDistributedCacheService();
                services.AddStackExchangeRedisCache(redisOptions =>
                {
                    redisOptions.InstanceName = "redis-name";
                    redisOptions.Configuration = options.CacheConnection;
                });
            }

            return services;
        }

        private static IServiceCollection AddInfasOpenTelemetry(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddTransient<IEventBus, EventBus>();

            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

            services.AddOpenTelemetry()
                     //.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WebApi"))
                     //.ConfigureResource(resource => resource.AddService("WebApi"))

                     .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WebApiTracing"))
                    .AddAspNetCoreInstrumentation() // Capture ASP.NET Core traces
                    .AddHttpClientInstrumentation() // Capture HttpClient traces
                                                    //.AddConsoleExporter();          // Export traces to the console
                   .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri("http://otel-collector:4317");
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
            })
            .WithMetrics(metricsProviderBuilder =>
            {
                metricsProviderBuilder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WebApiMetrics"))
                    .AddAspNetCoreInstrumentation() // Capture ASP.NET Core metrics
                                                    //.AddRuntimeInstrumentation()    // Capture runtime (GC, CPU, etc.) metrics
                    .AddPrometheusExporter()      // Export metrics to Prometheus
                                                  //.AddOtlpExporter(options =>
                                                  //    {
                                                  //        options.Endpoint = new Uri("http://otel-collector:4317");
                                                  //        options.Protocol = OtlpExportProtocol.Grpc;
                                                  //    })
                        ;
            })
            //.WithLogging(loggingBuilder =>
            //{
            //    loggingBuilder
            //        //.AddConsole()                   // Log to the console
            //        //.AddDebug()                     // Log to the Debug output (useful for development)
            //        .AddOpenTelemetry(options =>
            //        {
            //            options.IncludeScopes = true;    // Include scopes in logs
            //            options.ParseStateValues = true; // Parse structured logging values
            //            options.IncludeFormattedMessage = true; // Include formatted log messages
            //        });
            //})
                ;
            return services;
        }

        private static IServiceCollection AddConfigureMassTransit(this IServiceCollection services)
        {
            _ = services.AddMassTransit((busConfigurator) =>
            {
                busConfigurator.SetKebabCaseEndpointNameFormatter();

                //busConfigurator.AddConsumer<ProductCreatedEventConsumer, ProductCreatedEventConsumerDefinition>();
                //busConfigurator.AddConsumer<ProductCreatedEventConsumer>();
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
