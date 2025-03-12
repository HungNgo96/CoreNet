// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Asp.Versioning.ApiExplorer;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace WebApi.Extensions
{
    internal static class WebApplicationBuilderExtensions
    {
        internal static void UseConfigureSwagger(this WebApplication app)
        {
            bool isEnvProduct = app.Environment.IsProduction();
            var projectName = typeof(Program).Assembly.GetName().Name;

            if (isEnvProduct)
            {
                return;
            }

            _ = app.UseSwagger(c =>
            {
                c.RouteTemplate = "/v1/swagger/{documentName}/swagger.json";
            });

            _ = app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = "swagger";
                var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var groupName in provider.ApiVersionDescriptions.Select(x => x.GroupName))
                {
                    options.SwaggerEndpoint($"/v1/swagger/{groupName}/swagger.json",
                        projectName + " - " + groupName.ToUpperInvariant());
                }

                options.DisplayRequestDuration();
                options.EnableFilter();
                options.ShowExtensions();
                options.ShowCommonExtensions();
                options.EnableDeepLinking();
                options.DocExpansion(DocExpansion.None);
            });
        }

        internal static void AddJsonFiles(this WebApplicationBuilder builder)
        {
            builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
             .AddJsonFile("appsettings.json", true, true)
             .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
             .AddEnvironmentVariables();
        }

        internal static void UseHealthCheckCustom(this WebApplication app)
        {
            app.UseHealthChecks($"/health-checks", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                AllowCachingResponses = false,
            });
        }
    }
}
