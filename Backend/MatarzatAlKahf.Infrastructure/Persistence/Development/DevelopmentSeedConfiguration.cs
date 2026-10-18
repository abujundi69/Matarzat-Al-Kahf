using MatarzatAlKahf.Infrastructure.Features.Files.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MatarzatAlKahf.Infrastructure.Persistence.Development;

/// <summary>EF9 callbacks run under EF's migration lock, including Update-Database.</summary>
public static class DevelopmentSeedConfiguration
{
    public static void ConfigureDevelopmentSeed(
        this DbContextOptionsBuilder options,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (!IsEnabled(configuration, environment))
        {
            return;
        }

        options.UseSeeding((context, _) =>
        {
            // A deliberately targeted historical migration may not have the current
            // schema. Seed only after the full migration chain is applied.
            if (context.Database.GetPendingMigrations().Any())
            {
                return;
            }
            PrepareAssets(configuration, environment);
            new DevelopmentDataSeeder((MatarzatAlKahfDbContext)context).SeedDatabase();
        });
        options.UseAsyncSeeding(async (context, _, cancellationToken) =>
        {
            if ((await context.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                return;
            }
            PrepareAssets(configuration, environment);
            await new DevelopmentDataSeeder((MatarzatAlKahfDbContext)context)
                .SeedDatabaseAsync(cancellationToken);
        });
    }

    public static void PrepareAssets(IConfiguration configuration, IHostEnvironment environment)
    {
        if (IsEnabled(configuration, environment))
        {
            DevelopmentSeedAssets.EnsureAvailable(
                environment.ContentRootPath,
                FileStorageOptions.ResolveRootPath(
                    configuration[$"{FileStorageOptions.SectionName}:RootPath"],
                    environment.ContentRootPath));
        }
    }

    private static bool IsEnabled(IConfiguration configuration, IHostEnvironment environment) =>
        environment.IsDevelopment()
        && (configuration["DevelopmentSeed:Enabled"] is not string configured
            || bool.Parse(configured));
}
