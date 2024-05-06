using Application.Abstractions.Data;
using Application.Data;
using Application.Extensions;
using Domain.Core.SharedKernel.Correlation;
using Infrastructure.BackgroundJobs;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
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

    //_ = config.UseMicrosoftDependencyInjectionJobFactory();
});

services.AddQuartzHostedService();

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
