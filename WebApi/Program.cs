using Application.Abstractions.Data;
using Application.Data;
using Application.Extensions;
using Infrastructure.BackgroundJobs;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
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

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseErrorHandler();

app.UseAuthorization();

app.UseConfigureSwagger();

app.MapControllers();

app.Run();
