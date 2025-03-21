// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO.Compression;
using System.Net;
using System.Threading.RateLimiting;
using Application.Services;
using Asp.Versioning;
using Domain.Core;
using Domain.Core.AppSettings;
using Domain.Core.Extensions;
using Domain.Shared;
using Infrastructure.Extensions;
using Infrastructure.MessageBroker.RabbitMQ;
using Infrastructure.Telemetry;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;

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
              .AddOptionsWithValidation<ConnectionOptions>()
              .AddOptionsWithValidation<OpenTelemetryOptions>()
              .AddOptionsWithValidation<CacheOptions>()
              .AddOptionsWithValidation<RabbitMQOptions>();

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

        internal static IServiceCollection AddRateLimitRequest(this IServiceCollection services)
        {
            services.AddRateLimiter(config =>
            {
                //config.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                //{
                //    return RateLimitPartition.GetFixedWindowLimiter(partitionKey: httpContext.Request.Headers.Host.ToString(), partition =>
                //        new FixedWindowRateLimiterOptions
                //        {
                //            PermitLimit = 50,
                //            AutoReplenishment = true,
                //            Window = TimeSpan.FromSeconds(2),
                //            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                //            QueueLimit = 10
                //        });
                //});

                config.AddFixedWindowLimiter(policyName: "sample", options =>
                {
                    options.AutoReplenishment = true;
                    options.PermitLimit = 2;
                    options.Window = TimeSpan.FromSeconds(12);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 2;
                });

                var optionForPolicy = new FixedWindowRateLimiterOptions()
                {
                    AutoReplenishment = true,
                    PermitLimit = 20,
                    Window = TimeSpan.FromSeconds(5)
                };

                config.AddPolicy(policyName: "SamplePolicy", httpContext => RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
                        factory: _ => optionForPolicy));

                config.OnRejected = async (context, token) =>
                {
                    Result<string> responseModel = Result<string>.Fail("Too Many Requests", (int)HttpStatusCode.TooManyRequests);
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    await context.HttpContext.Response.WriteAsJsonAsync(responseModel, token);
                };

                #region example

                //// 1. Fixed Window Limiter (20 requests per 2 minutes)
                //// ----------------------------------------------
                //// | -------------------- 2 min -------------------- |
                //// | Blocked if limit hit                             |
                //// ----------------------------------------------
                //// Requests reset fully after 2 minutes, blocking all requests
                //// until the next window starts.
                config.AddFixedWindowLimiter("fixed", opt =>
                {
                    opt.Window = TimeSpan.FromMinutes(2);
                    opt.PermitLimit = 20;
                });

                //// 2. Sliding Window Limiter (20 requests per 2 minutes, 4 segments)
                //// ----------------------------------------------
                //// | --30s-- | --30s-- | --30s-- | --30s-- |  (4 segments in 2 min)
                //// |        <- Requests slide as segments roll over
                //// ----------------------------------------------
                //// As new 30-second segments open, the oldest one expires, allowing
                //// smoother traffic over time.
                config.AddSlidingWindowLimiter("sliding", opt =>
                {
                    opt.Window = TimeSpan.FromMinutes(2);
                    opt.PermitLimit = 20;
                    opt.SegmentsPerWindow = 4; // Divides into 4 rolling segments (every 30 seconds)
                });

                //// 3. Token Bucket Limiter (15 tokens max, refill 1 token/sec)
                //// ----------------------------------------------
                //// Tokens: 15 (Max) - Burst Capacity
                //// Refills: 1 token per second (smoother flow)
                //// ----------------------------------------------
                //// Allows bursts but gradually refills tokens over time.
                //// Requests consume tokens, and when depleted, requests are throttled until refilled.
                config.AddTokenBucketLimiter("token", opt =>
                {
                    opt.TokenLimit = 15;
                    opt.TokensPerPeriod = 1; // Refill rate
                    opt.ReplenishmentPeriod = TimeSpan.FromSeconds(1); // Refill interval
                });

                //// 4. Concurrency Limiter (5 concurrent requests, 10 in queue)
                //// ----------------------------------------------
                //// Concurrent: 1       (5 slots)
                //// Queue: | | | | | | | | | |    (10 slots)
                //// ----------------------------------------------
                //// Allows 5 active requests and queues 10 more. If the queue is full,
                //// new requests are rejected with 503 or 429 (if overridden).
                config.AddConcurrencyLimiter("concurrency", opt =>
                {
                    opt.PermitLimit = 5; // Max concurrent requests
                    opt.QueueLimit = 10; // Queue limit before rejection
                });

                #endregion example
            });
            return services;
        }
    }
}
