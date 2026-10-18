using System.Globalization;
using System.Threading.RateLimiting;
using MatarzatAlKahf.Api.Errors;
using Microsoft.AspNetCore.RateLimiting;

namespace MatarzatAlKahf.Api.RateLimiting;

internal static class RateLimitingServiceCollectionExtensions
{
    public static IServiceCollection AddLoginRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(
                "UnifiedLogin",
                httpContext => RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = configuration.GetValue(
                            "RateLimiting:UnifiedLoginPermitLimit",
                            5),
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1),
                    }));
            options.AddPolicy(
                "CustomerAuthentication",
                httpContext => RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1),
                    }));
            options.AddPolicy(
                "Otp",
                httpContext => RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 5,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1),
                    }));
            options.AddPolicy(
                "GuestOrderLookup",
                httpContext => RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(5),
                    }));
            options.AddPolicy(
                "Contact",
                httpContext => RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 3,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(10),
                    }));
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(
                        MetadataName.RetryAfter,
                        out TimeSpan retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(
                            retryAfter.TotalSeconds)
                        .ToString(CultureInfo.InvariantCulture);
                }

                await ApiProblemDetailsWriter.WriteAsync(
                    context.HttpContext,
                    StatusCodes.Status429TooManyRequests,
                    "Too many requests",
                    "Too many login attempts. Try again later.",
                    cancellationToken: cancellationToken);
            };
        });

        return services;
    }
}
