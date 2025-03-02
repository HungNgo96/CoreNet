// HungNgo96

using Contract.Abstractions.EventBus;
using Domain.Core.AppSettings;
using Domain.Core.Extensions;
using Infrastructure.MessageBroker;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.SqlClient;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;

namespace Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services
                .AddCacheService(builder.Configuration)
                .AddSetupOpenTelemetry(builder)
                .AddConfigureMassTransit();

            return services;
        }

        private static IServiceCollection AddCacheService(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionOptions = configuration.GetOptions<ConnectionOptions>() ?? new();
            string? redisConnection = connectionOptions.CacheConnection;
            if (connectionOptions.CacheConnectionInMemory())
            {
                //services.AddMemoryCacheService();
                services.AddMemoryCache(memoryOptions => memoryOptions.TrackStatistics = true);
            }
            else
            {
                //services.AddDistributedCacheService();
                //services.AddStackExchangeRedisCache(redisOptions =>
                //{
                //    redisOptions.InstanceName = "redis-name";
                //    redisOptions.Configuration = connectionOptions.CacheConnection;
                //});

                IFusionCacheBuilder builder = services.AddFusionCache().WithSystemTextJsonSerializer();
                if (!string.IsNullOrEmpty(redisConnection))
                {
                    builder.WithDistributedCache(new RedisCache(new RedisCacheOptions
                    {
                        Configuration = redisConnection
                    })).WithStackExchangeRedisBackplane(options =>
                    {
                        options.Configuration = redisConnection;
                    });
                }
            }

            return services;
        }

        private static IServiceCollection AddSetupOpenTelemetry(this IServiceCollection services, WebApplicationBuilder builder)
        {
            var otel = builder.Services.AddOpenTelemetry();
            using ServiceProvider provider = services.BuildServiceProvider();
            IHostEnvironment env = provider.GetRequiredService<IHostEnvironment>();

            builder.Logging.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.ParseStateValues = true;
                options.SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService("WebApiLogging"))
            .AddOtlpExporter(o =>
                    {
                        //connectionOptions.Endpoint = new Uri("http://otel-collector:4317");
                        //connectionOptions.Protocol = OtlpExportProtocol.Grpc;
                        o.Endpoint = new Uri("http://otel-collector:4318/v1/logs");
                        //o.Endpoint = new Uri("https://seq:5341/ingest/otlp/v1/logs ");
                        o.Protocol = OtlpExportProtocol.HttpProtobuf;
                        // Optional `X-Seq-ApiKey` header for authentication, if required
                        o.Headers = "X-Seq-ApiKey=abcde12345";
                    });

            });

            builder.Host.UseSerilog((context, config) =>
            {
                config
                    .WriteTo.Seq("http://seq:5341") // Gửi log về Seq
                    .WriteTo.OpenTelemetry(options =>
                    {
                        options.Endpoint = "http://otel-collector:4318/v1/logs";
                        options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.HttpProtobuf;
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
                        .AddSqlClientInstrumentation(delegate(SqlClientTraceInstrumentationOptions o)
                        {
                            o.RecordException = true;
                            if (!env.IsProduction())
                            {
                                o.SetDbStatementForText = true;
                            }
                        })
                        .AddOtlpExporter(options =>
                        {
                            //connectionOptions.Endpoint = new Uri("http://otel-collector:4317");
                            //connectionOptions.Protocol = OtlpExportProtocol.Grpc;
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
                        .AddPrometheusExporter() // Export metrics to Prometheus
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri("http://otel-collector:4318/v1/metrics");
                            options.Protocol = OtlpExportProtocol.HttpProtobuf;
                        })
                        // Metrics provides by ASP.NET Core in .NET 8

                        .AddMeter("Microsoft.AspNetCore.Hosting")
                        .AddMeter("Microsoft.AspNetCore.Server.Kestrel");
                });

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
                //       .Configure(connectionOptions =>
                //       {
                //           connectionOptions.WaitUntilStarted = true;
                //           connectionOptions.StartTimeout = TimeSpan.FromSeconds(30);
                //           connectionOptions.StopTimeout = TimeSpan.FromSeconds(60);
                //       });
                //services.AddOptions<HostOptions>()
                //    .Configure(connectionOptions =>
                //    {
                //        connectionOptions.StartupTimeout = TimeSpan.FromSeconds(60);
                //        connectionOptions.ShutdownTimeout = TimeSpan.FromSeconds(60);
                //    });
            });

            return services;
        }
    }
}
