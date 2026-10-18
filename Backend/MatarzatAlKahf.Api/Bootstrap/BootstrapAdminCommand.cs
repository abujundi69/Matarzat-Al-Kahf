using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Application.Common.Exceptions;

namespace MatarzatAlKahf.Api.Bootstrap;

internal static partial class BootstrapAdminCommand
{
    private const string CommandSwitch = "--bootstrap-admin";

    public static bool IsRequested(IEnumerable<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        return arguments.Any(IsCommandSwitch);
    }

    public static bool IsCommandSwitch(string argument) =>
        string.Equals(argument, CommandSwitch, StringComparison.OrdinalIgnoreCase);

    public static async Task<int> ExecuteAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);

        LogBootstrapStarted(logger);

        var request = new BootstrapAdminRequest(
            configuration["BootstrapAdmin:FullName"] ?? string.Empty,
            configuration["BootstrapAdmin:PhoneNumber"] ?? string.Empty,
            configuration["BootstrapAdmin:Email"],
            configuration["BootstrapAdmin:Password"] ?? string.Empty);

        try
        {
            await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
            IAdminBootstrapService bootstrapService =
                scope.ServiceProvider.GetRequiredService<IAdminBootstrapService>();
            BootstrapAdminResult result = await bootstrapService.BootstrapAsync(
                request,
                cancellationToken);

            LogBootstrapFinished(
                logger,
                result.Created,
                result.AdminUserId);
            return 0;
        }
        catch (ValidationException exception)
        {
            LogBootstrapValidationFailed(logger, exception.Message);
            return 1;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            LogBootstrapCancelled(logger);
            return 1;
        }
        catch (Exception exception)
        {
            LogBootstrapFailed(logger, exception.GetType().Name);
            return 1;
        }
    }

    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Administrator bootstrap started. The database must already exist and have all migrations applied")]
    private static partial void LogBootstrapStarted(ILogger logger);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Administrator bootstrap finished. Created: {Created}; Admin user: {AdminUserId}")]
    private static partial void LogBootstrapFinished(
        ILogger logger,
        bool created,
        Guid? adminUserId);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Error,
        Message = "Administrator bootstrap input is invalid: {Reason}")]
    private static partial void LogBootstrapValidationFailed(ILogger logger, string reason);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "Administrator bootstrap was cancelled")]
    private static partial void LogBootstrapCancelled(ILogger logger);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Error,
        Message = "Administrator bootstrap failed ({ErrorType}). Verify that the database exists and is migrated")]
    private static partial void LogBootstrapFailed(ILogger logger, string errorType);
}
