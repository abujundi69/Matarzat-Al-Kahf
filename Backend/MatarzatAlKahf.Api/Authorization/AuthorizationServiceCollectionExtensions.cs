using MatarzatAlKahf.Application.Common.Security;

namespace MatarzatAlKahf.Api.Authorization;

internal static class AuthorizationServiceCollectionExtensions
{
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                AuthenticationPolicyNames.Admin,
                policy => policy.RequireAuthenticatedUser()
                    .RequireClaim(AdminClaimTypes.AdminUserId));
            options.AddPolicy(
                AuthenticationPolicyNames.Customer,
                policy => policy.RequireAuthenticatedUser()
                    .RequireClaim(CustomerClaimTypes.CustomerId));
            foreach (string permissionCode in PermissionCodes.All)
            {
                AddPermissionPolicy(options, permissionCode, permissionCode);
            }

            AddPermissionPolicy(
                options,
                AuthorizationPolicyNames.CatalogCategoriesRead,
                PermissionCodes.CatalogCategoriesRead);
            AddPermissionPolicy(
                options,
                AuthorizationPolicyNames.CatalogCategoriesWrite,
                PermissionCodes.CatalogCategoriesWrite);
            AddPermissionPolicy(
                options,
                AuthorizationPolicyNames.CatalogProductsRead,
                PermissionCodes.CatalogProductsRead);
            AddPermissionPolicy(
                options,
                AuthorizationPolicyNames.CatalogProductsWrite,
                PermissionCodes.CatalogProductsWrite);
            AddPermissionPolicy(
                options,
                AuthorizationPolicyNames.CatalogInventoryRead,
                PermissionCodes.CatalogInventoryRead);
            AddPermissionPolicy(
                options,
                AuthorizationPolicyNames.CatalogInventoryWrite,
                PermissionCodes.CatalogInventoryWrite);
        });

        return services;
    }

    private static void AddPermissionPolicy(
        Microsoft.AspNetCore.Authorization.AuthorizationOptions options,
        string policyName,
        string permissionCode)
    {
        options.AddPolicy(
            policyName,
            policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(AdminClaimTypes.Permission, permissionCode));
    }
}
