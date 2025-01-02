// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO.Compression;
using Application.Services;
using Application.UseCases.v1.Products.Commands.CreateProduct;
using Asp.Versioning;
using Contract.Abstractions.EventBus;
using Contract.Abstractions.Idempotency;
using Domain.Core.AppSettings;
using Domain.Core.Extensions;
using Domain.Core.SharedKernel;
using Domain.Repositories;
using Infrastructure.BackgroundJobs;
using Infrastructure.Idempotency;
using Infrastructure.MessageBroker;
using MassTransit;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using Persistence.Repositories;
using Quartz;

namespace WebApi.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        internal static void AddRegisterSwagger(this IServiceCollection services, IWebHostEnvironment env)
        {
            if (!env.IsProduction())
            {
                services.AddSwaggerGen(c =>
                {
                    c.CustomSchemaIds(s => s.FullName?.Replace("+", "."));
                    c.EnableAnnotations();
                    c.UseInlineDefinitionsForEnums();
                    //Refer - https://gist.github.com/rafalkasa/01d5e3b265e5aa075678e0adfd54e23f
                    Uri url = new(uriString: "https://opensource.org/licenses/MIT");
                    // include all project's xml comments
                    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (!assembly.IsDynamic)
                        {
                            var xmlFile = $"{assembly.GetName().Name}.xml";
                            var xmlPath = Path.Combine(baseDirectory, xmlFile);
                            if (File.Exists(xmlPath))
                            {
                                c.IncludeXmlComments(xmlPath);
                            }
                        }
                    }

                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Version = "v1",
                        Title = "Title v1",
                        License = new OpenApiLicense
                        {
                            Name = "MIT License",
                            Url = url
                        }
                    });

                    //foreach (var swaggerDoc in SwaggerDocs())
                    //{
                    //    c.SwaggerDoc(swaggerDoc.Name, new OpenApiInfo
                    //    {
                    //        Version = swaggerDoc.Version,
                    //        Title = swaggerDoc.Title,
                    //        License = new OpenApiLicense
                    //        {
                    //            Name = "MIT License",
                    //            Url = url
                    //        }
                    //    });
                    //}

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        Description = "Input your Bearer token in this format - Bearer {your token here} to access this API",
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer",
                                },
                                Scheme = "Bearer",
                                Name = "Bearer",
                                In = ParameterLocation.Header,
                            }, new List<string>()
                        },
                    });
                });
            }
        }

        internal static IServiceCollection AddApiVersion(this IServiceCollection services)
        {
            services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
                config.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                                                                new HeaderApiVersionReader("x-api-version"),
                                                                new MediaTypeApiVersionReader("x-api-version"));
            }).AddApiExplorer(opts =>
            {
                opts.GroupNameFormat = "'v'VVV";
                opts.SubstituteApiVersionInUrl = true;
            });

            return services;
        }

       

        internal static IServiceCollection AddConfigureMassTransit(this IServiceCollection services)
        {
            _ = services.AddMassTransit((busConfigurator) =>
            {
                busConfigurator.SetKebabCaseEndpointNameFormatter();

                //busConfigurator.AddConsumer<ProductCreatedEventConsumer, ProductCreatedEventConsumerDefinition>();
                //busConfigurator.AddConsumers(Assembly.GetExecutingAssembly());
                busConfigurator.UsingRabbitMq((context, configurator) =>
                {
                    configurator.PrefetchCount = 7;//to consumer in order but this is not performance
                    configurator.Host(new Uri("amqp://localhost:5672"), (h) =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    //configurator.ReceiveEndpoint("product-service", e =>
                    //{
                    //    e.ConcurrentMessageLimit = 28; // only applies to this endpoint
                    //    e.PrefetchCount = 5;
                    //    e.ConfigureConsumer<ProductCreatedEventConsumer>(context);
                    //});
                    configurator.Message<ProductCreatedEvent>(x =>
                    {
                        x.SetEntityName("product-created-event-exchange-2");
                    });
                    configurator.ConfigureEndpoints(context);
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

            services.AddTransient<IEventBus, EventBus>();
            return services;
        }

        internal static IServiceCollection AddConfigQuartz(this IServiceCollection services)
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

        internal static IServiceCollection AddConfigResponseCompression(this IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
#if DEBUG
                options.EnableForHttps = true;
#endif
                options.Providers.Add<BrotliCompressionProvider>();//priority 1
                options.Providers.Add<GzipCompressionProvider>();//priority 2
            });

            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.SmallestSize;
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.SmallestSize;
            });

            return services;
        }

        internal static IServiceCollection AddCurrentUserService(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            return services;
        }

        internal static IServiceCollection AddConfigOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions()
                .Configure<ConnectionOptions>(configuration.GetRequiredSection("ConnectionStrings"));

            return services;
        }

        internal static IServiceCollection AddHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            //IConfigurationSection appSettingKafka = configuration.GetSection("Kafka");
            //IConfigurationSection appSettingRedis = configuration.GetSection("Redis");
            //var redisOption = appSettingRedis.Get<RedisConfigOptions>();
            //var kafkaOption = appSettingKafka.Get<KafkaConfigOptions>();

            //services.AddHostedService<StartupHostedService>()
            //        .AddSingleton<StartupHostedServiceHealthCheck>();

            //services
            //       .AddHealthChecks()
            //       .AddRedis(redisOption.ConfigRedis, tags: new[] { "system" })
            //       .AddKafka(config =>
            //       {
            //           config.BatchSize = kafkaOption.Serilog.BatchSizeLimit;
            //           config.BootstrapServers = kafkaOption.Serilog.Brokers;
            //       }, kafkaOption.Serilog.TopicRequest, tags: new[] { "system" })
            //       .AddCheck<SRWebHealthChecks>("SRWeb API", tags: new[] { "app" })
            //       .AddCheck<InsideHealthChecks>("Inside", tags: new[] { "app" })
            //       .AddCheck<TransactionCounterHealthChecks>("Transaction Counter", tags: new[] { "app" })
            //       .AddCheck<StartupHostedServiceHealthCheck>("hosted_service_startup", failureStatus: HealthStatus.Degraded, tags: new[] { "ready" });

            //services.AddHealthChecksUI(config =>
            //{
            //    config.DisableDatabaseMigrations();
            //    config.SetEvaluationTimeInSeconds(360);
            //    config.SetHeaderText("SRMobi");
            //    config.SetMinimumSecondsBetweenFailureNotifications(60);
            //}).AddInMemoryStorage();

            return services;
        }

        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            services.AddScoped<IProductRepository, ProductRepository>();
            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IIdempotencyService, IdempotencyService>();

            return services;
        }

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
    }
}
