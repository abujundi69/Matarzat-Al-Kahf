using System.Data;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Maintenance;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Persistence.Maintenance;

/// <summary>
/// Removes sample and development business data so the database holds only what a live store
/// needs. Schema, constraints, indexes, the permission catalogue, the two active system roles,
/// notification templates, and the store/SEO configuration rows are preserved; only their
/// development-only values are cleared.
/// </summary>
public sealed class DemoDataPurger(MatarzatAlKahfDbContext dbContext) : IDemoDataPurger
{
    /// <summary>
    /// The development seed gives every sample administrator an address in this reserved mDNS
    /// domain, which cannot resolve to a real mailbox. It is therefore a safe marker for "this
    /// account came from the sample data" that never matches an operator's own account.
    /// </summary>
    private const string DevelopmentEmailDomain = "@matarzatalkahf.local";

    /// <summary>
    /// Settings whose seeded values describe a development environment. The rows stay so the
    /// administration screens keep their structure; the values are cleared so nothing fake is
    /// ever shown to a shopper.
    /// </summary>
    private static readonly string[] DemoValuedSettingKeys =
    [
        "store.logoUrl",
        "store.faviconUrl",
        "contact.email",
        "contact.phone",
        "contact.whatsapp",
    ];

    /// <summary>
    /// Offline payment methods whose seeded instructions point at development accounts. They are
    /// disabled until an operator supplies real transfer details, because an enabled method with
    /// no instructions would leave a shopper unable to pay.
    /// </summary>
    private static readonly string[] UnconfiguredPaymentMethodKeys =
    [
        "payment-method.banktransfer",
        "payment-method.electronicwallet",
        "payment-method.creditcard",
    ];

    private readonly MatarzatAlKahfDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public Task<DemoDataPurgeResult> PurgeAsync(CancellationToken cancellationToken) =>
        DbTransactionExecutor.ExecuteAsync(
            _dbContext,
            PurgeCoreAsync,
            cancellationToken,
            IsolationLevel.Serializable);

    private async Task<DemoDataPurgeResult> PurgeCoreAsync(CancellationToken cancellationToken)
    {
        // Promotions reference catalog rows, so usage and scope links go before the coupons.
        await _dbContext.CouponUsages.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.CouponCategories.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.CouponProducts.ExecuteDeleteAsync(cancellationToken);
        int coupons = await _dbContext.Coupons.ExecuteDeleteAsync(cancellationToken);

        // Every row that points at an order goes first; the order foreign keys are NO ACTION, so
        // SQL Server rejects the parent delete while a replay record or payment still refers to
        // it.
        await _dbContext.CheckoutIdempotencies.ExecuteDeleteAsync(cancellationToken);
        int payments = await _dbContext.Payments.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.OrderStatusHistory.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.OrderItems.ExecuteDeleteAsync(cancellationToken);
        int orders = await _dbContext.Orders.ExecuteDeleteAsync(cancellationToken);

        await _dbContext.CartItems.ExecuteDeleteAsync(cancellationToken);
        int carts = await _dbContext.Carts.ExecuteDeleteAsync(cancellationToken);

        await _dbContext.InventoryTransactions.ExecuteDeleteAsync(cancellationToken);
        int inventoryRecords = await _dbContext.InventoryItems.ExecuteDeleteAsync(
            cancellationToken);

        await _dbContext.ProductImageTranslations.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.ProductImages.ExecuteDeleteAsync(cancellationToken);
        int productVariants = await _dbContext.ProductVariants.ExecuteDeleteAsync(
            cancellationToken);
        await _dbContext.ProductTranslations.ExecuteDeleteAsync(cancellationToken);
        int products = await _dbContext.Products.ExecuteDeleteAsync(cancellationToken);

        await _dbContext.CategoryTranslations.ExecuteDeleteAsync(cancellationToken);
        int categories = await _dbContext.Categories.ExecuteDeleteAsync(cancellationToken);

        int contactMessages = await _dbContext.ContactMessages.ExecuteDeleteAsync(
            cancellationToken);
        await _dbContext.BannerTranslations.ExecuteDeleteAsync(cancellationToken);
        int banners = await _dbContext.Banners.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.ContentPageTranslations.ExecuteDeleteAsync(cancellationToken);
        int contentPages = await _dbContext.ContentPages.ExecuteDeleteAsync(cancellationToken);
        int socialLinks = await _dbContext.SocialMediaLinks.ExecuteDeleteAsync(cancellationToken);

        await _dbContext.CustomerSessions.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.OtpVerifications.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.CustomerAddresses.ExecuteDeleteAsync(cancellationToken);
        int customers = await _dbContext.Customers.ExecuteDeleteAsync(cancellationToken);

        int notifications = await _dbContext.Notifications.ExecuteDeleteAsync(cancellationToken);
        int auditLogs = await _dbContext.AuditLogs.ExecuteDeleteAsync(cancellationToken);
        await _dbContext.ImportOperations.ExecuteDeleteAsync(cancellationToken);
        int storedFiles = await _dbContext.StoredFiles.ExecuteDeleteAsync(cancellationToken);

        // Delivery areas carry prices and minimums that only the store owner can set, so the
        // sample areas go rather than shipping invented fees to real shoppers.
        int deliveryLocations = await _dbContext.DeliveryLocations.ExecuteDeleteAsync(
            cancellationToken);

        (int adminUsers, int retainedSuperAdmins) = await PurgeAdminUsersAsync(cancellationToken);
        int retiredRoles = await PurgeRetiredRolesAsync(cancellationToken);
        int neutralizedSettings = await NeutralizeDevelopmentConfigurationAsync(cancellationToken);

        int retainedPermissions = await _dbContext.Permissions.CountAsync(cancellationToken);
        int retainedTemplates = await _dbContext.NotificationTemplates.CountAsync(
            cancellationToken);

        return new DemoDataPurgeResult(
            categories,
            products,
            productVariants,
            inventoryRecords,
            customers,
            carts,
            orders,
            payments,
            coupons,
            banners,
            contentPages,
            contactMessages,
            socialLinks,
            deliveryLocations,
            notifications,
            auditLogs,
            storedFiles,
            adminUsers,
            retiredRoles,
            neutralizedSettings,
            retainedPermissions,
            retainedTemplates,
            retainedSuperAdmins);
    }

    /// <summary>
    /// Removes the administrative accounts the development seed created, including its
    /// SuperAdmin, because their passwords are published in the repository documentation. They
    /// are identified by the seed's reserved <c>.local</c> email domain, which can never belong
    /// to a real mailbox, so an account an operator created is never touched. When no SuperAdmin
    /// survives, the caller is told to run the bootstrap command.
    /// </summary>
    private async Task<(int Removed, int RetainedSuperAdmins)> PurgeAdminUsersAsync(
        CancellationToken cancellationToken)
    {
        Guid[] seededIds = await _dbContext.AdminUsers
            .Where(user => user.Email != null
                && user.Email.EndsWith(DevelopmentEmailDomain))
            .Select(user => user.Id)
            .ToArrayAsync(cancellationToken);

        if (seededIds.Length > 0)
        {
            await _dbContext.AdminSessions
                .Where(session => seededIds.Contains(session.AdminUserId))
                .ExecuteDeleteAsync(cancellationToken);
            await _dbContext.AdminUserRoles
                .Where(mapping => seededIds.Contains(mapping.AdminUserId))
                .ExecuteDeleteAsync(cancellationToken);
        }

        int removed = seededIds.Length == 0
            ? 0
            : await _dbContext.AdminUsers
                .Where(user => seededIds.Contains(user.Id))
                .ExecuteDeleteAsync(cancellationToken);

        int retainedSuperAdmins = await _dbContext.AdminUsers
            .CountAsync(
                user => !user.IsDeleted
                    && user.UserRoles.Any(mapping =>
                        mapping.Role.Name == SystemRoleNames.SuperAdmin),
                cancellationToken);

        return (removed, retainedSuperAdmins);
    }

    /// <summary>
    /// Drops the role rows the application retired. Only <c>SuperAdmin</c> and <c>Admin</c> are
    /// assignable, so the remaining sample roles are unreachable data that still appeared in
    /// role filters and reports.
    /// </summary>
    private async Task<int> PurgeRetiredRolesAsync(CancellationToken cancellationToken)
    {
        Guid[] retiredRoleIds = await _dbContext.Roles
            .Where(role => role.Name != SystemRoleNames.SuperAdmin
                && role.Name != SystemRoleNames.Admin)
            .Select(role => role.Id)
            .ToArrayAsync(cancellationToken);
        if (retiredRoleIds.Length == 0)
        {
            return 0;
        }

        await _dbContext.RolePermissions
            .Where(mapping => retiredRoleIds.Contains(mapping.RoleId))
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.AdminUserRoles
            .Where(mapping => retiredRoleIds.Contains(mapping.RoleId))
            .ExecuteDeleteAsync(cancellationToken);
        return await _dbContext.Roles
            .Where(role => retiredRoleIds.Contains(role.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// Clears configuration values that only make sense on a developer machine. The rows survive
    /// so the settings screens stay complete and an operator can fill in real values.
    /// </summary>
    private async Task<int> NeutralizeDevelopmentConfigurationAsync(
        CancellationToken cancellationToken)
    {
        int cleared = 0;
        List<StoreSetting> settings = await _dbContext.StoreSettings
            .Where(setting => DemoValuedSettingKeys.Contains(setting.Key)
                || UnconfiguredPaymentMethodKeys.Contains(setting.Key))
            .ToListAsync(cancellationToken);
        Guid[] settingIds = settings.Select(setting => setting.Id).ToArray();
        List<StoreSettingTranslation> translations = await _dbContext.StoreSettingTranslations
            .Where(translation => settingIds.Contains(translation.StoreSettingId))
            .ToListAsync(cancellationToken);
        DateTime now = DateTime.UtcNow;

        foreach (StoreSetting setting in settings)
        {
            bool isPaymentMethod = UnconfiguredPaymentMethodKeys.Contains(
                setting.Key,
                StringComparer.Ordinal);
            string nextValue = isPaymentMethod ? bool.FalseString : string.Empty;
            if (!string.Equals(setting.Value, nextValue, StringComparison.Ordinal))
            {
                setting.Value = nextValue;
                setting.UpdatedAtUtc = now;
                cleared++;
            }

            foreach (StoreSettingTranslation translation in translations
                .Where(candidate => candidate.StoreSettingId == setting.Id))
            {
                if (translation.Value.Length == 0)
                {
                    continue;
                }

                translation.Value = string.Empty;
                translation.UpdatedAtUtc = now;
            }
        }

        SeoSetting? seo = await _dbContext.SeoSettings.OrderBy(item => item.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);
        if (seo is not null)
        {
            // The seeded base URL points at the Vite dev server, and robots.txt plus sitemap.xml
            // are generated from it, so leaving it would publish localhost links.
            if (!string.IsNullOrEmpty(seo.BaseUrl))
            {
                seo.BaseUrl = string.Empty;
                cleared++;
            }

            if (seo.ArabicOpenGraphImageUrl.Length > 0
                || seo.EnglishOpenGraphImageUrl.Length > 0)
            {
                seo.ArabicOpenGraphImageUrl = string.Empty;
                seo.EnglishOpenGraphImageUrl = string.Empty;
                cleared++;
            }

            seo.UpdatedAtUtc = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return cleared;
    }
}
