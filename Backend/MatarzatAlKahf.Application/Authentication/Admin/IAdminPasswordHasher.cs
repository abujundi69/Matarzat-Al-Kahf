using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Authentication.Admin;
public interface IAdminPasswordHasher
{
    string HashPassword(AdminUser adminUser, string password);
    bool VerifyPassword(AdminUser adminUser, string providedPassword);
}
