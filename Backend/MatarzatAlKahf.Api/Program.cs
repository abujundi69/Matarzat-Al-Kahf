using System.Diagnostics;
using System.Text.Json.Serialization;
using MatarzatAlKahf.Api;
using MatarzatAlKahf.Api.Authentication;
using MatarzatAlKahf.Api.Authorization;
using MatarzatAlKahf.Api.Bootstrap;
using MatarzatAlKahf.Api.Errors;
using MatarzatAlKahf.Api.Localization;
using MatarzatAlKahf.Api.OpenApi;
using MatarzatAlKahf.Api.RateLimiting;
using MatarzatAlKahf.Api.Security;
using MatarzatAlKahf.Api.Uploads;
using MatarzatAlKahf.Application;
using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
using MatarzatAlKahf.Infrastructure;
using MatarzatAlKahf.Infrastructure.Persistence.Development;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection.Extensions;

bool isBootstrapAdminCommand = BootstrapAdminCommand.IsRequested(args);
bool isDevelopmentSeedCommand = DevelopmentSeedCommand.IsRequested(args);
bool isPurgeDemoDataCommand = PurgeDemoDataCommand.IsRequested(args);
bool isMaintenanceCommand =
    isBootstrapAdminCommand || isDevelopmentSeedCommand || isPurgeDemoDataCommand;
string[] hostArguments = args
    .Where(argument =>
        !BootstrapAdminCommand.IsCommandSwitch(argument)
        && !DevelopmentSeedCommand.IsCommandSwitch(argument)
        && !PurgeDemoDataCommand.IsCommandSwitch(argument))
    .ToArray();

var builder = WebApplication.CreateBuilder(hostArguments);

// Machine-specific configuration stays outside source control. Environment variables
// remain the final override, including in CI and isolated verification databases.
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile(
        "appsettings.Development.local.json", optional: true, reloadOnChange: false);
    builder.Configuration.AddUserSecrets<MatarzatAlKahfDbContextFactory>(optional: true);
}
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddCommandLine(hostArguments);

// The server name and version tell an attacker which exploits to try and help no legitimate client.
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Logging.Configure(options =>
    options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId
        | ActivityTrackingOptions.SpanId
        | ActivityTrackingOptions.ParentId);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddPermissionAuthorization();
builder.Services.AddApiServices(builder.Configuration);

if (builder.Environment.IsDevelopment()
    && builder.Configuration.GetValue<bool>("Otp:EnableTestProvider"))
{
    builder.Services.Replace(
        ServiceDescriptor.Scoped<IOtpDeliveryService, DevelopmentOtpDeliveryService>());
}

if (!isMaintenanceCommand)
{
    builder.Services.AddApiJwtBearer(builder.Configuration);
}
else
{
    builder.Services.AddSingleton<IAdminTokenService, BootstrapDisabledAdminTokenService>();
    builder.Services.AddSingleton<ICustomerTokenService, BootstrapDisabledCustomerTokenService>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    DevelopmentSeedConfiguration.PrepareAssets(builder.Configuration, app.Environment);
}

if (isDevelopmentSeedCommand)
{
    Environment.ExitCode = await DevelopmentSeedCommand.ExecuteAsync(
        app.Services,
        app.Environment,
        app.Logger,
        CancellationToken.None);
    return;
}

if (isPurgeDemoDataCommand)
{
    Environment.ExitCode = await PurgeDemoDataCommand.ExecuteAsync(
        app.Services,
        app.Logger,
        CancellationToken.None);
    return;
}

if (isBootstrapAdminCommand)
{
    Environment.ExitCode = await BootstrapAdminCommand.ExecuteAsync(
        app.Services,
        builder.Configuration,
        app.Logger,
        CancellationToken.None);
    return;
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UsePublicUploadFiles(builder.Configuration);
app.UseRouting();
app.UseCors(MatarzatAlKahf.Api.DependencyInjection.FrontendCorsPolicy);
app.UseRequestLocalization();
app.UseMiddleware<ContentLanguageMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Matarzat Al Kahf API v1"));
}

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("live"),
        ResponseWriter = static (context, _) =>
            context.Response.WriteAsJsonAsync(new { status = "Healthy" })
    });
app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("ready"),
        ResponseWriter = static (context, report) =>
            context.Response.WriteAsJsonAsync(new
            {
                status = report.Status.ToString()
            })
    });

app.Run();
