// HungNgo96

using Domain.Core.AppSettings;
using Domain.Core.Extensions;
using Domain.Core.SharedKernel;
using Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence.DbContexts;

namespace Persistence.DependencyInjections.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfigDbContext(this IServiceCollection services, IConfiguration config)
        {
            var optionsConfig = config.GetOptions<ConnectionOptions>() ?? new();
            services.AddSingleton<InsertOutboxMessageInterceptor>();

            services.AddDbContext<ReadApplicationDbContext>((sp, op) =>
            {
                Console.WriteLine(optionsConfig.ReadSqlServer);
                op.UseSqlServer(optionsConfig.ReadSqlServer!, x =>
                {
                    x.MigrationsAssembly("Persistence");
                    x.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                });

                op.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

                var environment = sp.GetRequiredService<IHostEnvironment>();

                if (!environment.IsProduction())
                {
                    op.EnableDetailedErrors();
                    op.EnableSensitiveDataLogging();
                }
            }, contextLifetime: ServiceLifetime.Scoped);

            services.AddDbContext<WriteApplicationDbContext>((sp, op) =>
            {
                Console.WriteLine(optionsConfig.WriteSqlServer);
                op.UseSqlServer(optionsConfig.WriteSqlServer!, x =>
                {
                    x.MigrationsAssembly("Persistence");
                    x.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                });

                var environment = sp.GetRequiredService<IHostEnvironment>();

                if (!environment.IsProduction())
                {
                    op.EnableDetailedErrors();
                    op.EnableSensitiveDataLogging();
                }

                op.AddInterceptors(sp.GetRequiredService<InsertOutboxMessageInterceptor>());
            }, contextLifetime: ServiceLifetime.Scoped);

            services.AddScoped<IReadApplicationDbContext>(s => s.GetRequiredService<ReadApplicationDbContext>());
            services.AddScoped<IWriteApplicationDbContext>(s => s.GetRequiredService<WriteApplicationDbContext>());
            services.AddScoped<IUnitOfWork>(s => s.GetRequiredService<WriteApplicationDbContext>());

            return services;
        }
    }
}
