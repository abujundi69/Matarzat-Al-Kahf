using MatarzatAlKahf.Infrastructure.Persistence.Development;

namespace MatarzatAlKahf.Api.Bootstrap;

internal static partial class DevelopmentSeedCommand
{
    private const string CommandSwitch = "--seed-development";

    public static bool IsRequested(IEnumerable<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        return arguments.Any(IsCommandSwitch);
    }

    public static bool IsCommandSwitch(string argument) =>
        string.Equals(
            argument,
            CommandSwitch,
            StringComparison.OrdinalIgnoreCase);

    public static async Task<int> ExecuteAsync(
        IServiceProvider serviceProvider,
        IHostEnvironment hostEnvironment,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(hostEnvironment);
        ArgumentNullException.ThrowIfNull(logger);

        string? aspNetCoreEnvironment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (!hostEnvironment.IsDevelopment()
            || !string.Equals(
                aspNetCoreEnvironment,
                Environments.Development,
                StringComparison.OrdinalIgnoreCase))
        {
            LogRejected(
                logger,
                aspNetCoreEnvironment ?? "(not set)",
                hostEnvironment.EnvironmentName);
            return 2;
        }

        LogStarted(logger);
        try
        {
            await using AsyncServiceScope scope =
                serviceProvider.CreateAsyncScope();
            IDevelopmentDataSeeder seeder =
                scope.ServiceProvider.GetRequiredService<
                    IDevelopmentDataSeeder>();
            DevelopmentSeedResult result = await seeder.SeedAsync(
                cancellationToken);
            LogFinished(
                logger,
                result.Permissions,
                result.Roles,
                result.AdminUsers,
                result.DeliveryLocations,
                result.Categories,
                result.Products,
                result.ProductVariants,
                result.Customers,
                result.Orders,
                result.Notifications,
                result.ContentPages);
            return 0;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            LogCancelled(logger);
            return 1;
        }
        catch (Exception exception)
        {
            LogFailed(logger, exception.Message, exception);
            return 1;
        }
    }

    [LoggerMessage(
        EventId = 1200,
        Level = LogLevel.Information,
        Message = "Development data seed started. Migrations must already be applied")]
    private static partial void LogStarted(ILogger logger);

    [LoggerMessage(
        EventId = 1201,
        Level = LogLevel.Information,
        Message = "Development data seed finished: {Permissions} permissions, {Roles} roles, {AdminUsers} administrators, {DeliveryLocations} delivery locations, {Categories} categories, {Products} products, {ProductVariants} variants, {Customers} customers, {Orders} orders, {Notifications} notifications, and {ContentPages} content pages")]
    private static partial void LogFinished(
        ILogger logger,
        int permissions,
        int roles,
        int adminUsers,
        int deliveryLocations,
        int categories,
        int products,
        int productVariants,
        int customers,
        int orders,
        int notifications,
        int contentPages);

    [LoggerMessage(
        EventId = 1202,
        Level = LogLevel.Error,
        Message = "Development seed rejected. ASPNETCORE_ENVIRONMENT must be Development (actual: {AspNetCoreEnvironment}; host environment: {HostEnvironment})")]
    private static partial void LogRejected(
        ILogger logger,
        string aspNetCoreEnvironment,
        string hostEnvironment);

    [LoggerMessage(
        EventId = 1203,
        Level = LogLevel.Warning,
        Message = "Development data seed was cancelled")]
    private static partial void LogCancelled(ILogger logger);

    [LoggerMessage(
        EventId = 1204,
        Level = LogLevel.Error,
        Message = "Development data seed failed: {Reason}")]
    private static partial void LogFailed(
        ILogger logger,
        string reason,
        Exception exception);
}
