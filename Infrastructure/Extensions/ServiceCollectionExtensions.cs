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
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using OpenTelemetry;
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
            var otel = builder.Services.AddOpenTelemetry();
            builder.Logging.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService("WebApiLogging")
            .AddAttributes(new Dictionary<string, object>
            {
                ["service.environment"] = "development"
            }))
                 .AddOtlpExporter(o =>
                    {
                        //options.Endpoint = new Uri("http://otel-collector:4317");
                        //options.Protocol = OtlpExportProtocol.Grpc;
                        o.Endpoint = new Uri("http://otel-collector:4318/v1/logs");
                        //o.Endpoint = new Uri("http://seq:5341/ingest/otlp/v1/logs");
                        o.Protocol = OtlpExportProtocol.HttpProtobuf;
                    });
            });


            //.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WebApi"))
            //.ConfigureResource(resource => resource.AddService("WebApi"))

            otel.WithTracing(tracerProviderBuilder =>
           {
               tracerProviderBuilder
                   .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WebApiTracing"))
                   .AddAspNetCoreInstrumentation() // Capture ASP.NET Core traces
                   .AddHttpClientInstrumentation() // Capture HttpClient traces
                   .AddSqlClientInstrumentation()
                   .AddOtlpExporter(options =>
                       {
                           //options.Endpoint = new Uri("http://otel-collector:4317");
                           //options.Protocol = OtlpExportProtocol.Grpc;
                           options.Endpoint = new Uri("http://otel-collector:4318/v1/traces");
                           options.Protocol = OtlpExportProtocol.HttpProtobuf;
                       }).AddZipkinExporter(options =>
                       {
                           options.Endpoint = new Uri("http://zipkin:9411");
                       });
           })
           .WithMetrics(metricsProviderBuilder =>
           {
               metricsProviderBuilder
                       .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("WebApiMetrics"))
                       .AddAspNetCoreInstrumentation() // Capture ASP.NET Core metrics
                                                       //.AddRuntimeInstrumentation()    // Capture runtime (GC, CPU, etc.) metrics
                       .AddPrometheusExporter()      // Export metrics to Prometheus
                       .AddOtlpExporter(options =>
                         {
                             options.Endpoint = new Uri("http://otel-collector:4318/v1/metrics");
                             options.Protocol = OtlpExportProtocol.HttpProtobuf;
                         })
                       // Metrics provides by ASP.NET Core in .NET 8

                       .AddMeter("Microsoft.AspNetCore.Hosting")
                       .AddMeter("Microsoft.AspNetCore.Server.Kestrel");

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

            //otel.UseOtlpExporter();//set default or detail above
            return services;
        }

        private static IServiceCollection AddConfigureMassTransit(this IServiceCollection services)
        {
            services.AddTransient<IEventBus, EventBus>();

            _ = services.AddMassTransit((busConfigurator) =>
            {
                busConfigurator.SetKebabCaseEndpointNameFormatter();

                //busConfigurator.AddConsumer<ProductCreatedEventConsumer, ProductCreatedEventConsumerDefinition>();
                //busConfigurator.AddConsumer<ProductCreatedEventConsumer>();
                //busConfigurator.AddConsumers(Assembly.GetExecutingAssembly());
                busConfigurator.UsingRabbitMq((context, configurator) =>
                {
                    configurator.PrefetchCount = 7;//to consumer in order but this is not performance
                    configurator.Host(new Uri("amqp://rabbitmq:5672"), (h) =>
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
