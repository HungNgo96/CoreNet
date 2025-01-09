// HungNgo96

using Contract.Abstractions.EventBus;
using Domain.Core.AppSettings;
using Domain.Core.Extensions;
using Infrastructure.MessageBroker;
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
        public static IServiceCollection AddCacheService(this IServiceCollection services, IConfiguration configuration)
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

        public static IServiceCollection AddInfasOpenTelemetry(this IServiceCollection services, WebApplicationBuilder builder)
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
    }
}
