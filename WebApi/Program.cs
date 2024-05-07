using Application.Abstractions.Data;
using Application.Abstractions.EventBus;
using Application.Data;
using Application.Extensions;
using Application.Products.Commands.CreateProduct;
using Domain.Core.SharedKernel.Correlation;
using Infrastructure.BackgroundJobs;
using Infrastructure.Extensions;
using Infrastructure.MessageBroker;
using Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Quartz;
using WebApi.Extensions;
using WebApi.Middlewares;

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

services.AddControllers();

services.AddRegisterSwagger(env);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddApiVersion();

services.AddApplication()
    .AddInfrastructure();

services.AddConfigDbContext(configuration);

services.AddScoped<ICorrelationIdGenerator, CorrelationIdGenerator>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

services.AddConfigQuartz();

services.AddConfigureMassTransit();

var app = builder.Build();

app.UseForwardedHeaders();

app.UseCorrelationId();

app.UseErrorHandler();

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseConfigureSwagger();
}

app.MapControllers();

app.Run();
