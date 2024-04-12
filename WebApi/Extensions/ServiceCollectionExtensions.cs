// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Asp.Versioning;
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
                    c.EnableAnnotations();
                    //TODO - Lowercase Swagger Documents
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
    }
}
