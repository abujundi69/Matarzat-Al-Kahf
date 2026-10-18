using System.Data;
using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MatarzatAlKahf.Infrastructure.Authentication.Admin;
public sealed partial class AdminBootstrapService(MatarzatAlKahfDbContext dbContext, IPhoneNumberNormalizer phoneNumberNormalizer, IPasswordPolicy passwordPolicy, IAdminPasswordHasher passwordHasher, ILogger<AdminBootstrapService> logger) : IAdminBootstrapService
{
    private static readonly Action<ILogger, string, Guid?, Exception?> LogBootstrapCompleted = LoggerMessage.Define<string, Guid?>(LogLevel.Information, new EventId(1101, nameof(LogBootstrapCompleted)), "Initial administrator bootstrap completed with outcome {Outcome} for admin {AdminUserId}");
    private static readonly Action<ILogger, Exception?> LogBootstrapConflict = LoggerMessage.Define(LogLevel.Warning, new EventId(1102, nameof(LogBootstrapConflict)), "Initial administrator bootstrap encountered a data conflict");
    private static readonly RoleSeed[] RequiredRoles =
    [
        new(SystemRoleNames.SuperAdmin, "Full administrative access.", "مدير النظام الأعلى", "SuperAdmin"),
        new(SystemRoleNames.Admin, "Full day-to-day administrative access.", "مدير", "Admin")
    ];
    private static readonly PermissionSeed[] RequiredPermissions = [new(PermissionCodes.CatalogCategoriesRead, "Read categories", "View category data."), new(PermissionCodes.CatalogCategoriesWrite, "Write categories", "Create, update, and delete categories."), new(PermissionCodes.CatalogProductsRead, "Read products", "View product data."), new(PermissionCodes.CatalogProductsWrite, "Write products", "Create, update, and delete products."), new(PermissionCodes.CatalogInventoryRead, "Read inventory", "View inventory data."), new(PermissionCodes.CatalogInventoryWrite, "Write inventory", "Adjust inventory quantities."), ..PermissionCodes.All.Except([PermissionCodes.CatalogCategoriesRead, PermissionCodes.CatalogCategoriesWrite, PermissionCodes.CatalogProductsRead, PermissionCodes.CatalogProductsWrite, PermissionCodes.CatalogInventoryRead, PermissionCodes.CatalogInventoryWrite], StringComparer.Ordinal).Select(code => new PermissionSeed(code, code, $"Allows {code}.")).ToArray()];
    private readonly MatarzatAlKahfDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly IPhoneNumberNormalizer _phoneNumberNormalizer = phoneNumberNormalizer ?? throw new ArgumentNullException(nameof(phoneNumberNormalizer));
    private readonly IPasswordPolicy _passwordPolicy = passwordPolicy ?? throw new ArgumentNullException(nameof(passwordPolicy));
    private readonly IAdminPasswordHasher _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    private readonly ILogger<AdminBootstrapService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    public async Task<BootstrapAdminResult> BootstrapAsync(BootstrapAdminRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request);
        string normalizedPhoneNumber = _phoneNumberNormalizer.Normalize(request.PhoneNumber);
        try
        {
            BootstrapAdminResult result = await DbTransactionExecutor.ExecuteAsync(_dbContext, token => BootstrapCoreAsync(request, normalizedPhoneNumber, token), cancellationToken, IsolationLevel.Serializable);
            LogBootstrapCompleted(_logger, result.Outcome, result.AdminUserId, null);
            return result;
        }
        catch (DbUpdateException exception)when (DbUpdateExceptionTranslator.IsUniqueViolation(exception))
        {
            LogBootstrapConflict(_logger, exception);
            throw new ConflictException("Administrator, role, or permission data already exists.", exception);
        }
    }

    private sealed record RoleSeed(
        string Name,
        string Description,
        string DisplayNameArabic,
        string DisplayNameEnglish);
    private sealed record PermissionSeed(string Code, string Name, string Description);
}
