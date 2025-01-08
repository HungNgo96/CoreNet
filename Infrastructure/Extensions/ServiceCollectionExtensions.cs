// HungNgo96

using Infrastructure.BackgroundJobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Quartz;

namespace Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfigQuartz(this IServiceCollection services)
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

        public static IServiceCollection AddInfasOpenTelemetry(this IServiceCollection services, WebApplicationBuilder builder)
        {
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
