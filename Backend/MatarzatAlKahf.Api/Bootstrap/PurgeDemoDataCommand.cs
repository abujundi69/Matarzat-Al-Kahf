using MatarzatAlKahf.Application.Maintenance;

namespace MatarzatAlKahf.Api.Bootstrap;

/// <summary>
/// Explicit maintenance command that strips sample and development business data from an existing
/// database. It is destructive, so it only ever runs when an operator passes the switch; normal
/// API startup never invokes it.
/// </summary>
internal static partial class PurgeDemoDataCommand
{
    private const string CommandSwitch = "--purge-demo-data";

    public static bool IsRequested(IEnumerable<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        return arguments.Any(IsCommandSwitch);
    }

    public static bool IsCommandSwitch(string argument) =>
        string.Equals(argument, CommandSwitch, StringComparison.OrdinalIgnoreCase);

    public static async Task<int> ExecuteAsync(
        IServiceProvider serviceProvider,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(logger);

        LogStarted(logger);
        try
        {
            await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
            IDemoDataPurger purger = scope.ServiceProvider
                .GetRequiredService<IDemoDataPurger>();
            DemoDataPurgeResult result = await purger.PurgeAsync(cancellationToken);
            LogRemoved(
                logger,
                result.Categories,
                result.Products,
                result.ProductVariants,
                result.InventoryRecords,
                result.Customers,
                result.Carts,
                result.Orders,
                result.Payments,
                result.Coupons);
            LogRemovedContent(
                logger,
                result.Banners,
                result.ContentPages,
                result.ContactMessages,
                result.SocialLinks,
                result.DeliveryLocations,
                result.Notifications,
                result.AuditLogs,
                result.StoredFiles);
            LogIdentity(
                logger,
                result.AdminUsers,
                result.RetiredRoles,
                result.RetainedSuperAdmins);
            LogRetained(
                logger,
                result.NeutralizedSettings,
                result.RetainedPermissions,
                result.RetainedNotificationTemplates);
            if (result.RetainedSuperAdmins == 0)
            {
                LogNoSuperAdmin(logger);
            }

            return 0;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
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
        EventId = 1300,
        Level = LogLevel.Information,
        Message = "Demo data purge started. Migrations must already be applied")]
    private static partial void LogStarted(ILogger logger);

    [LoggerMessage(
        EventId = 1301,
        Level = LogLevel.Information,
        Message = "Removed business data: {Categories} categories, {Products} products, {Variants} variants, {Inventory} inventory records, {Customers} customers, {Carts} carts, {Orders} orders, {Payments} payments, {Coupons} coupons")]
    private static partial void LogRemoved(
        ILogger logger,
        int categories,
        int products,
        int variants,
        int inventory,
        int customers,
        int carts,
        int orders,
        int payments,
        int coupons);

    [LoggerMessage(
        EventId = 1302,
        Level = LogLevel.Information,
        Message = "Removed content and operational data: {Banners} banners, {Pages} content pages, {Messages} contact messages, {SocialLinks} social links, {DeliveryLocations} delivery locations, {Notifications} notifications, {AuditLogs} audit entries, {StoredFiles} stored files")]
    private static partial void LogRemovedContent(
        ILogger logger,
        int banners,
        int pages,
        int messages,
        int socialLinks,
        int deliveryLocations,
        int notifications,
        int auditLogs,
        int storedFiles);

    [LoggerMessage(
        EventId = 1303,
        Level = LogLevel.Information,
        Message = "Identity cleanup: removed {AdminUsers} administrators and {RetiredRoles} retired roles; retained {SuperAdmins} SuperAdmin account(s)")]
    private static partial void LogIdentity(
        ILogger logger,
        int adminUsers,
        int retiredRoles,
        int superAdmins);

    [LoggerMessage(
        EventId = 1304,
        Level = LogLevel.Information,
        Message = "Cleared {NeutralizedSettings} development configuration value(s); retained {Permissions} permissions and {Templates} notification templates")]
    private static partial void LogRetained(
        ILogger logger,
        int neutralizedSettings,
        int permissions,
        int templates);

    [LoggerMessage(
        EventId = 1305,
        Level = LogLevel.Warning,
        Message = "No SuperAdmin account remains. Run --bootstrap-admin before serving traffic")]
    private static partial void LogNoSuperAdmin(ILogger logger);

    [LoggerMessage(
        EventId = 1306,
        Level = LogLevel.Warning,
        Message = "Demo data purge was cancelled")]
    private static partial void LogCancelled(ILogger logger);

    [LoggerMessage(
        EventId = 1307,
        Level = LogLevel.Error,
        Message = "Demo data purge failed: {Reason}")]
    private static partial void LogFailed(
        ILogger logger,
        string reason,
        Exception exception);
}
