namespace MatarzatAlKahf.Application.Maintenance;

/// <summary>
/// What a purge removed. Counts are reported so an operator can confirm the database reached the
/// expected state instead of trusting the command silently.
/// </summary>
public sealed record DemoDataPurgeResult(
    int Categories,
    int Products,
    int ProductVariants,
    int InventoryRecords,
    int Customers,
    int Carts,
    int Orders,
    int Payments,
    int Coupons,
    int Banners,
    int ContentPages,
    int ContactMessages,
    int SocialLinks,
    int DeliveryLocations,
    int Notifications,
    int AuditLogs,
    int StoredFiles,
    int AdminUsers,
    int RetiredRoles,
    int NeutralizedSettings,
    int RetainedPermissions,
    int RetainedNotificationTemplates,
    int RetainedSuperAdmins);
