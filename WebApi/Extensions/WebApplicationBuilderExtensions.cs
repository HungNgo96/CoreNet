// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace WebApi.Extensions
{
    internal static class WebApplicationBuilderExtensions
    {
        internal static void UseSerilog(this WebApplicationBuilder app)
        {
            app.Logging.ClearProviders();
            //var serilogConfig = app.Configuration.GetRequiredSection("Kafka:Serilog").Get<KafkaOptions>();
            app.Host.UseSerilog((_, __, configuration) =>
                configuration
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
                    outputTemplate: "[{Timestamp:dd-MM-yyyy HH:mm:ss.fff}] [{MachineName} - {EnvironmentName}] | [{EventType:x8}{Level:u3}] [{SourceContext}] [ThreadId:{ThreadId}] [EventId:{EventId}] {CorrelationId} {UserInfo} {ConnectionId} {ClientAgent}{NewLine} {Message:lj}{NewLine} {Exception}{NewLine}",
                    theme: AnsiConsoleTheme.Code)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                    .MinimumLevel.Override("System", LogEventLevel.Error))
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
                );
        }

        internal static void UseConfigureSwagger(this WebApplication app)
        {
            bool isEnvProduct = app.Environment.IsProduction();
            var projectName = typeof(Program).Assembly.GetName().Name;

            if (!isEnvProduct)
            {
                _ = app.UseSwagger(c =>
                {
                    c.RouteTemplate = "/{documentName}/swagger.json";
                });

                _ = app.UseSwaggerUI(options =>
                {
                    options.RoutePrefix = "swagger";

                    options.SwaggerEndpoint($"/v1/swagger.json", $"{projectName} -- v1");
                    options.DisplayRequestDuration();
                    options.EnableFilter();
                    options.ShowExtensions();
                    options.ShowCommonExtensions();
                    options.EnableDeepLinking();
                    options.DocExpansion(DocExpansion.None);
                });
            }
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
            app.UseHealthChecks($"/healthchecks", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                AllowCachingResponses = false,
            });
        }
    }
}
