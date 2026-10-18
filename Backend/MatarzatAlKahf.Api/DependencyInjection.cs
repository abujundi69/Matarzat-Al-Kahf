using MatarzatAlKahf.Api.Errors;
using MatarzatAlKahf.Api.Health;
using MatarzatAlKahf.Api.Localization;
using MatarzatAlKahf.Api.OpenApi;
using MatarzatAlKahf.Api.RateLimiting;
using MatarzatAlKahf.Api.Security;
using MatarzatAlKahf.Application.Common.Security;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MatarzatAlKahf.Api;

internal static class DependencyInjection
{
    public const string FrontendCorsPolicy = "FrontendCors";

    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApiProblemDetails();
        services.AddApiLocalization(configuration);
        services.AddCurrentLanguage();
        services.AddLoginRateLimiting(configuration);
        services.AddEndpointsApiExplorer();
        services.AddApiSwagger();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentActor, HttpCurrentActor>();
        services.AddHealthChecks()
            .AddCheck("live", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

        string[] origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy =>
        {
            if (origins.Length > 0)
            {
                policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
            }
        }));

        return services;
    }
}
