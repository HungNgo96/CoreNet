// HungNgo96

using Infrastructure.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;
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

        public static IServiceCollection AddInfasOpenTelemetry(this IServiceCollection services)
        {
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService("MyDotNetApp"))

             .WithTracing(tracerProviderBuilder =>
             {
                 tracerProviderBuilder
                     .AddAspNetCoreInstrumentation()
                     .AddHttpClientInstrumentation()
                     .AddConsoleExporter(); // Xuất trace ra console
             })
             .WithMetrics(metricsProviderBuilder =>
             {
                 metricsProviderBuilder
                     .AddAspNetCoreInstrumentation()
                     .AddPrometheusExporter(); // Xuất metric cho Prometheus
             });
            return services;
        }
    }
}
