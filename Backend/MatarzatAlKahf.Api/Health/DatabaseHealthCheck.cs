using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MatarzatAlKahf.Api.Health;

internal sealed class DatabaseHealthCheck(
    MatarzatAlKahfDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Database is unavailable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Database is unavailable.", exception);
        }
    }
}
