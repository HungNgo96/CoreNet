using Application.Abstractions.Data;
using Application.Extensions;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Presentation;
using Presentation.Extensions;
using Serilog;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication()
    .AddInfrastructure()
    .AddPresentation();

builder.Services.AddDbContext<ReadApplicationDbContext>(op =>
{
    op.UseSqlServer(config.GetRequiredSection("ConnectionStrings:ReadSqlServer").Value, x => x.MigrationsAssembly("Presentation"));
    op.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
}, contextLifetime: ServiceLifetime.Scoped);

builder.Services.AddDbContext<WriteApplicationDbContext>(op =>
{
    op.UseSqlServer(config.GetRequiredSection("ConnectionStrings:WriteSqlServer").Value, x => x.MigrationsAssembly("Presentation"));
}, contextLifetime: ServiceLifetime.Scoped);

builder.Services.AddScoped<IReadApplicationDbContext>(s => s.GetRequiredService<ReadApplicationDbContext>());
builder.Services.AddScoped<IWriteApplicationDbContext>(s => s.GetRequiredService<WriteApplicationDbContext>());

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseAuthorization();

app.MapControllers();

app.Run();
