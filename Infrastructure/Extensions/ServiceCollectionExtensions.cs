// HungNgo96

using Contract.Abstractions.EventBus;
using Domain.Core.AppSettings;
using Domain.Core.Extensions;
using Google.Protobuf.WellKnownTypes;
using Infrastructure.Constants;
using Infrastructure.MessageBroker;
using Infrastructure.MessageBroker.RabbitMQ;
using Infrastructure.Telemetry;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.SqlClient;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using ZiggyCreatures.Caching.Fusion;

namespace Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services
                .AddCacheService(builder.Configuration)
                .AddSetupOpenTelemetry(builder, builder.Configuration)
                .AddConfigureMassTransit(builder.Configuration);

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

        private static IServiceCollection AddSetupOpenTelemetry(this IServiceCollection services, WebApplicationBuilder builder, IConfiguration configuration)
        {
            var openTel = builder.Services.AddOpenTelemetry();
            using var provider = services.BuildServiceProvider();
            var env = provider.GetRequiredService<IHostEnvironment>();
            var openTelemetryOptions = configuration.GetOptions<OpenTelemetryOptions>() ?? new();

            if (openTelemetryOptions.Logging.Enabled)
            {
                if (openTelemetryOptions.Logging.Exporter == OpenTelConst.Exporters.Oltp)
                {
                    openTel.WithLogging(delegate (LoggerProviderBuilder b)
                                  {
                                      b.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService($"{openTelemetryOptions.ServiceName}_logging"));
                                      b.AddOtlpExporter(delegate (OtlpExporterOptions o)
                                      {
                                          o.Endpoint = new Uri(openTelemetryOptions.Logging.Otlp.Endpoint);
                                          o.Protocol = openTelemetryOptions.Logging.Otlp.Protocol == OpenTelConst.Protocols.Grpc
                                              ? OtlpExportProtocol.Grpc
                                              : OtlpExportProtocol.HttpProtobuf;
                                      });
                                  }, delegate (OpenTelemetryLoggerOptions o)
                                  {
                                      o.IncludeScopes = true;
                                      o.IncludeFormattedMessage = true;
                                      o.ParseStateValues = true;
                                  });
                }
                else
                {
                    builder.Logging.ClearProviders();
                    builder.Host.UseSerilog((_, config) =>
                    {
                        config
                            .MinimumLevel.Information()
                            .Enrich.FromLogContext()
                            //.Enrich.With<RequestContextEnricher>()
                            //.Enrich.WithThreadId()//{ThreadId}
                            //.Enrich.WithThreadName()//{ThreadName}
                            //.Enrich.WithEnvironmentName()
                            //.Enrich.WithMachineName()//{ThreadName}
                            //.Enrich.WithClientIp()
                            //.Enrich.WithClientAgent()
                            .WriteTo.Logger(
                                lc => lc.WriteTo.Console(
                                    outputTemplate:
                                    "[{Timestamp:dd-MM-yyyy HH:mm:ss.fff}] [{MachineName} - {EnvironmentName}] | [{EventType:x8}{Level:u3}] [{SourceContext}] [ThreadId:{ThreadId}] [EventId:{EventId}] {CorrelationId} {UserInfo} {ConnectionId} {ClientAgent}{NewLine} {Message:lj}{NewLine} {Exception}{NewLine}",
                                    theme: AnsiConsoleTheme.Code)
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                            .MinimumLevel.Override("System", LogEventLevel.Error)
                            );
                        //.WriteTo.Logger(lc => lc
                        //    .Filter.ByExcluding(e => Matching.FromSource("System")(e) && e.Level <= LogEventLevel.Information)
                        //    .Filter.ByExcluding(e => Matching.FromSource("Microsoft")(e) && e.Level <= LogEventLevel.Information)
                        //    .WriteTo.Kafka(bootstrapServers: serilogConfig!.Brokers,
                        //        batchSizeLimit: serilogConfig.BatchSizeLimit,
                        //        period: serilogConfig.Period,
                        //        saslMechanism: SaslMechanism.Plain,
                        //        topic: serilogConfig.TopicRequest,
                        //        formatter: new KafkaLogFormatter())
                        //)

                        config
                            .WriteTo.OpenTelemetry(options =>
                            {
                                options.Endpoint = openTelemetryOptions.Logging.Otlp.Endpoint;
                                options.Protocol = openTelemetryOptions.Logging.Otlp.Protocol == OpenTelConst.Protocols.Grpc
                                    ? Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc
                                    : Serilog.Sinks.OpenTelemetry.OtlpProtocol.HttpProtobuf;

                                options.ResourceAttributes = new Dictionary<string, object>
                                {
                                    ["service.name"] = openTelemetryOptions.ServiceName,
                                    ["service.version"] = "1.0.0"
                                };
                            });
                    });
                }
            }

            if (openTelemetryOptions.Tracing.Enabled)
            {
                openTel.WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault()
                            .AddService($"{openTelemetryOptions.ServiceName}_tracing"))
                        .AddAspNetCoreInstrumentation() // Capture ASP.NET Core traces
                        .AddHttpClientInstrumentation() // Capture HttpClient traces
                        .AddSqlClientInstrumentation(delegate (SqlClientTraceInstrumentationOptions o)
                        {
                            o.RecordException = true;
                            if (!env.IsProduction())
                            {
                                o.SetDbStatementForText = true;
                                o.RecordException = true;
                            }
                        })
                        .AddOtlpExporter(options =>
                        {
                            //connectionOptions.Endpoint = new Uri("http://openTel-collector:4317");
                            //connectionOptions.Protocol = OtlpExportProtocol.Grpc;
                            //options.Endpoint = new Uri("http://openTel-collector:4318/v1/traces");
                            //options.Protocol = OtlpExportProtocol.HttpProtobuf;
                            options.Endpoint = new Uri(openTelemetryOptions.Tracing.Otlp.Endpoint);
                            options.Protocol = openTelemetryOptions.Tracing.Otlp.Protocol == OpenTelConst.Protocols.Grpc
                                ? OtlpExportProtocol.Grpc
                                : OtlpExportProtocol.HttpProtobuf;
                        })
                        .AddZipkinExporter(options =>
                        {
                            options.Endpoint = new Uri("http://zipkin:9411");
                        });
                });
            }

            if (openTelemetryOptions.Metrics.Enabled)
            {
                openTel.WithMetrics(metricsProviderBuilder =>
               {
                   metricsProviderBuilder
                       .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService($"{openTelemetryOptions.ServiceName}_metrics"))
                       .AddAspNetCoreInstrumentation() // Capture ASP.NET Core metrics
                       .AddRuntimeInstrumentation()    // Capture runtime (GC, CPU, etc.) metrics
                       .AddPrometheusExporter() // Export metrics to Prometheus
                       .AddOtlpExporter(options =>
                       {
                           //options.Endpoint = new Uri("http://openTel-collector:4318/v1/metrics");
                           //////http://openTel-collector:4318/v1/metrics
                           //options.Protocol = OtlpExportProtocol.HttpProtobuf;
                           options.Endpoint = new Uri(openTelemetryOptions.Metrics.Otlp.Endpoint);
                           options.Protocol = openTelemetryOptions.Metrics.Otlp.Protocol == OpenTelConst.Protocols.Grpc
                           ? OtlpExportProtocol.Grpc
                           : OtlpExportProtocol.HttpProtobuf;
                       })
                       // Metrics provides by ASP.NET Core in .NET 8
                       .AddMeter("Microsoft.AspNetCore.Hosting")
                       .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                           // demo
                        .AddMeter("MyApp.Metrics")
                       ;
               });
            }

            //openTel.UseOtlpExporter();//set default or detail above
            return services;
        }

        private static IServiceCollection AddConfigureMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            var rabbitMQOptions = configuration.GetOptions<RabbitMQOptions>() ?? new();
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
                    configurator.Host(new Uri($"amqp://{rabbitMQOptions.HostName}:{rabbitMQOptions.Port}"), (h) =>
                    {
                        h.Username(rabbitMQOptions.UserName);
                        h.Password(rabbitMQOptions.Password);
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
