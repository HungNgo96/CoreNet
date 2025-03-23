using System.Text.Json.Serialization;
using Application.DependencyInjections.Extensions;
using Contract.Interfaces;
using FluentValidation.AspNetCore;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using WebApi.Commons;
using WebApi.ConfigOptions;
using WebApi.Extensions;
using WebApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var env = builder.Environment;
var configuration = builder.Configuration;

builder.AddJsonFiles();
// Add services to the container.
services.AddControllers((options) =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new KebabParameterTransformer()));
})
.AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)
.ConfigureApiBehaviorOptions(ConfigureApiBehaviorExtension.ConfigureApiBehavior);

services.AddRequestDecompression()
        .AddConfigResponseCompression();

services.AddExceptionHandler<GlobalExceptionHandler>();

services.AddHealthCheck(configuration);

services.AddRegisterSwagger(env);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddApiVersion();

services.AddCurrentUserService();

services.AddConfigOptions(configuration);

services.AddApplication()
    .AddInfrastructureLayer(builder)
    .AddPersistenceLayer(configuration);

services.AddFluentValidationAutoValidation();//fluent API

services.AddCorrelationGenerator();
 
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.Scan(scan => scan
    .FromAssemblies(AppDomain.CurrentDomain.GetAssemblies()) // hoặc typeof(Program).Assembly
    .AddClasses(classes => classes.AssignableTo<IScopedService>())
        .AsImplementedInterfaces()
        .WithScopedLifetime()
    .AddClasses(classes => classes.AssignableTo<ISingletonService>())
        .AsImplementedInterfaces()
        .WithSingletonLifetime()
    .AddClasses(classes => classes.AssignableTo<ITransientService>())
        .AsImplementedInterfaces()
        .WithTransientLifetime()
);

var app = builder.Build();

app.UseForwardedHeaders();

app.UseCorrelationId();

app.UseErrorHandler();

app.UseRouting();

app.UseAuthorization();

app.UseRequestDecompression();

app.UseResponseCompression();

app.UseStaticFiles();

app.UseHealthCheckCustom();

app.UseConfigureSwagger();

// Kích hoạt Prometheus metrics endpoint
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapControllers();

//if (app.Environment.IsDevelopment())
//{
//    using var scope = app.Services.CreateScope();
//    var dbContext = scope.ServiceProvider.GetRequiredService<WriteApplicationDbContext>();

//    if (!await dbContext.Database.CanConnectAsync(CancellationToken.None))
//    {
//        throw new ConnectionException("Couldn't connect database.");
//    }

//    //await dbContext.Database.EnsureCreatedAsync();
//    await dbContext.Database.MigrateAsync();
//}


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
