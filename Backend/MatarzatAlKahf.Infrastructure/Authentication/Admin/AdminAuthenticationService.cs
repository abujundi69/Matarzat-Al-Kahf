using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Responses;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MatarzatAlKahf.Infrastructure.Authentication.Admin;

public sealed class AdminAuthenticationService(
    MatarzatAlKahfDbContext dbContext,
    IPhoneNumberNormalizer phoneNumberNormalizer,
    IAdminPasswordHasher passwordHasher,
    IAdminTokenService tokenService,
    ILogger<AdminAuthenticationService> logger)
    : IAdminAuthenticationService
{
    private const string InvalidCredentialsMessage = "Invalid login credentials.";

    private static readonly Action<ILogger, string, Exception?> LogLoginFailedByIdentifier =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(1001, nameof(LogLoginFailedByIdentifier)),
            "Administrative login failed for an identifier of type {IdentifierType}");
    private static readonly Action<ILogger, Guid, Exception?> LogLoginFailedById =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            new EventId(1002, nameof(LogLoginFailedById)),
            "Administrative login failed for admin {AdminUserId}");
    private static readonly Action<ILogger, Guid, Exception?> LogLoginSucceeded =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(1003, nameof(LogLoginSucceeded)),
            "Administrative login succeeded for admin {AdminUserId}");

    private readonly MatarzatAlKahfDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly IPhoneNumberNormalizer _phoneNumberNormalizer =
        phoneNumberNormalizer ?? throw new ArgumentNullException(nameof(phoneNumberNormalizer));
    private readonly IAdminPasswordHasher _passwordHasher =
        passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    private readonly IAdminTokenService _tokenService =
        tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    private readonly ILogger<AdminAuthenticationService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<AdminLoginResponse> LoginAsync(
        AdminLoginRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.PhoneNumber)
            || string.IsNullOrEmpty(request.Password))
        {
            LogLoginFailedByIdentifier(_logger, "missing", null);
            throw new AuthenticationFailedException(InvalidCredentialsMessage);
        }

        string normalizedPhoneNumber;
        try
        {
            normalizedPhoneNumber = _phoneNumberNormalizer.Normalize(request.PhoneNumber);
        }
        catch (ValidationException)
        {
            LogLoginFailedByIdentifier(_logger, "invalid", null);
            throw new AuthenticationFailedException(InvalidCredentialsMessage);
        }

        IQueryable<AdminUser> users = _dbContext.AdminUsers
            .Include(user => user.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .ThenInclude(role => role.RolePermissions)
            .ThenInclude(rolePermission => rolePermission.Permission);
        AdminUser? adminUser = await users.SingleOrDefaultAsync(
            user => user.NormalizedPhoneNumber == normalizedPhoneNumber,
            cancellationToken);

        if (adminUser is null)
        {
            LogLoginFailedByIdentifier(
                _logger,
                "phone",
                null);
            throw new AuthenticationFailedException(InvalidCredentialsMessage);
        }

        bool passwordIsValid = _passwordHasher.VerifyPassword(adminUser, request.Password);

        try
        {
            AdminAuthenticationRules.EnsureLoginAllowed(
                adminUser.Status,
                adminUser.IsDeleted,
                passwordIsValid);
        }
        catch (AuthenticationFailedException)
        {
            LogLoginFailedById(_logger, adminUser.Id, null);
            throw;
        }

        bool isSuperAdmin = adminUser.UserRoles.Any(userRole =>
            userRole.Role.Name == SystemRoleNames.SuperAdmin);
        string effectiveRoleName = isSuperAdmin
            ? SystemRoleNames.SuperAdmin
            : SystemRoleNames.Admin;
        Role? effectiveRole = adminUser.UserRoles
            .Select(userRole => userRole.Role)
            .FirstOrDefault(role => role.Name == effectiveRoleName);
        string[] roles = [effectiveRoleName];
        AdminRoleSummaryResponse[] roleSummaries =
        [
            new AdminRoleSummaryResponse(
                effectiveRole?.Id ?? Guid.Empty,
                effectiveRoleName)
            {
                DisplayNameArabic = effectiveRole?.DisplayNameArabic
                    ?? (isSuperAdmin ? "مدير النظام الأعلى" : "مدير"),
                DisplayNameEnglish = effectiveRole?.DisplayNameEnglish
                    ?? effectiveRoleName
            }
        ];
        string[] permissions = AdminPermissionResolver.Resolve(adminUser);

        var descriptor = new AdminTokenDescriptor(
            adminUser.Id,
            adminUser.FullName,
            adminUser.PhoneNumber,
            adminUser.PreferredLanguage,
            roles,
            permissions);
        AccessToken token = _tokenService.CreateAccessToken(descriptor);

        DateTime now = DateTime.UtcNow;
        adminUser.LastLoginAtUtc = now;
        adminUser.UpdatedAtUtc = now;
        // Administrative tokens are tracked so logout, suspension, deletion, and password
        // resets can revoke them instead of leaving full access live until expiry.
        _dbContext.AdminSessions.Add(new AdminSession
        {
            AdminUserId = adminUser.Id,
            TokenIdHash = AdminSessionTokenHasher.Hash(token.TokenId),
            ExpiresAtUtc = token.ExpiresAtUtc
        });
        await _dbContext.SaveChangesAsync(cancellationToken);

        LogLoginSucceeded(_logger, adminUser.Id, null);

        return new AdminLoginResponse(
            token.Value,
            AccessToken.BearerTokenType,
            token.ExpiresAtUtc,
            new AdminProfileDto(
                adminUser.Id,
                adminUser.FullName,
                adminUser.PhoneNumber,
                adminUser.Email,
                adminUser.PreferredLanguage,
                roles,
                permissions)
            {
                RoleSummaries = roleSummaries
            });
    }
}
