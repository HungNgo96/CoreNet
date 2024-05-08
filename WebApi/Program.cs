using System.Text.Json.Serialization;
using Application.Extensions;
using Domain.Core.SharedKernel.Correlation;
using FluentValidation.AspNetCore;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using WebApi.ConfigOptions;
using WebApi.Extensions;
using WebApi.Middlewares;
using Domain.Core;
using System.Threading;
using Infrastructure.Persistence.Idempotency;
using Application.Abstractions.Idempotency;
using Domain.Repositories;
using Infrastructure.Persistence.Repositories;
//IConfiguration config = new ConfigurationBuilder()
//    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
//    .AddEnvironmentVariables()
//    .Build();
var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var env = builder.Environment;
var configuration = builder.Configuration;

builder.AddJsonFiles();
// Add services to the container.
builder.UseSerilog();

services.AddControllers()
.AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)
.ConfigureApiBehaviorOptions(ConfigureApiBehaviorExtension.ConfigureApiBehavior);

services.AddRequestDecompression()
        .AddConfigResponseCompression();

services.AddHealthCheck(configuration);

services.AddRegisterSwagger(env);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddApiVersion();

services.AddCurrentUserService();

services.AddApplication()
    .AddInfrastructure();

services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();//fluent API

services.AddConfigDbContext(configuration);

services.AddCorrelationGenerator();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

services.AddConfigQuartz();
services.AddConfigureMassTransit();

//services.AddRepository(typeof(Enti)).AddServices(services);

services.AddScoped<IIdempotencyService, IdempotencyService>();
services.AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();

app.UseForwardedHeaders();

app.UseCorrelationId();

app.UseErrorHandler();

app.UseAuthorization();

app.UseRequestDecompression();

app.UseResponseCompression();

app.UseStaticFiles();

app.UseHealthCheckCustom();

if (app.Environment.IsDevelopment())
{
    var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<WriteApplicationDbContext>();


    if (!await dbContext.Database.CanConnectAsync(default))
    {
        throw new Exception("Couldn't connect database.");
    }
    dbContext.Database.Migrate();
    app.UseConfigureSwagger();
}

app.MapControllers();
CheckTime(app);
await app.RunAsync();


#region private

void CheckTime(WebApplication app)
{
    ILogger<Program> _ilogger = app.Services.GetRequiredService<ILogger<Program>>();
    var now = DateTime.Now;
    var utcNow = DateTime.UtcNow;
    var currentTimeZone = TimeZoneInfo.Local;
    var currentTimeZoneUtc = TimeZoneInfo.Utc;

    _ilogger.LogInformation(@"----------------Local time--------------------
    Local Now: {Now} -- Kind :{Kind}
    TimeZone Local: DaylightName: {DaylightName} -- DisplayName: {DisplayName}
    ", now, now.Kind, currentTimeZone.DaylightName, currentTimeZone.DisplayName);

    _ilogger.LogInformation(@"----------------UTC time----------------------
    UTC Now: {UtcNow} -- Kind: {Kind}
    TimeZone Utc: DaylightName: {DaylightName} -- DisplayName: {DisplayName}
    ", utcNow, utcNow.Kind, currentTimeZoneUtc.DaylightName, currentTimeZoneUtc.DisplayName);
}

#endregion private
