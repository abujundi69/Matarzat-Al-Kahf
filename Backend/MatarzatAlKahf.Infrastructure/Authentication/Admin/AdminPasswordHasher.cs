using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace MatarzatAlKahf.Infrastructure.Authentication.Admin;

public sealed class AdminPasswordHasher : IAdminPasswordHasher
{
    private readonly PasswordHasher<AdminUser> _passwordHasher = new();

    public string HashPassword(AdminUser adminUser, string password)
    {
        ArgumentNullException.ThrowIfNull(adminUser);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return _passwordHasher.HashPassword(adminUser, password);
    }

    public bool VerifyPassword(AdminUser adminUser, string providedPassword)
    {
        ArgumentNullException.ThrowIfNull(adminUser);
        ArgumentNullException.ThrowIfNull(providedPassword);

        return _passwordHasher.VerifyHashedPassword(
                adminUser,
                adminUser.PasswordHash,
                providedPassword)
            != PasswordVerificationResult.Failed;
    }
}
