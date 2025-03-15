// HungNgo96

using Contract.Abstractions.Idempotency;
using Domain.Core.AppSettings;
using Domain.Core.Extensions;
using Domain.Core.SharedKernel;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence.DbContexts;
using Persistence.Idempotency;
using Persistence.Interceptors;
using Persistence.Repositories;

namespace Persistence.DependencyInjections.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddConfigDbContext(configuration)
            .AddRepository()
            .AddServices();

            return services;
        }

        private static IServiceCollection AddConfigDbContext(this IServiceCollection services, IConfiguration config)
        {
            var optionsConfig = config.GetOptions<ConnectionOptions>() ?? new();
            services.AddSingleton<InsertOutboxMessageInterceptor>();

            services.AddDbContext<ReadApplicationDbContext>((sp, op) =>
            {
                op.UseSqlServer(optionsConfig.ReadSqlServer!, x =>
                {
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
                    x.MigrationsAssembly(typeof(WriteApplicationDbContext).Assembly.FullName);
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

        private static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            services.AddScoped<IProductRepository, ProductRepository>();
            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IIdempotencyService, IdempotencyService>();

            return services;
        }
    }
}
