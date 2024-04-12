using Application.Abstractions.Data;
using Application.Extensions;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using WebApi.Extensions;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var env = builder.Environment;
var configuration = builder.Configuration;
// Add services to the container.
builder.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddRegisterSwagger(env);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddApiVersion();

builder.Services.AddApplication()
    .AddInfrastructure();

builder.Services.AddDbContext<ReadApplicationDbContext>(op =>
{
    op.UseSqlServer(config.GetRequiredSection("ConnectionStrings:ReadSqlServer").Value, x => x.MigrationsAssembly("Infrastructure"));
    op.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
}, contextLifetime: ServiceLifetime.Scoped);

builder.Services.AddDbContext<WriteApplicationDbContext>(op =>
{
    op.UseSqlServer(config.GetRequiredSection("ConnectionStrings:WriteSqlServer").Value, x => x.MigrationsAssembly("Infrastructure"));
}, contextLifetime: ServiceLifetime.Scoped);

builder.Services.AddScoped<IReadApplicationDbContext>(s => s.GetRequiredService<ReadApplicationDbContext>());
builder.Services.AddScoped<IWriteApplicationDbContext>(s => s.GetRequiredService<WriteApplicationDbContext>());


var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseAuthorization();

app.UseConfigureSwagger();

app.MapControllers();

app.Run();
