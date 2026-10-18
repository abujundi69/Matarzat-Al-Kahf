using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Authentication.Admin;
public static class AdminAuthenticationRules
{
    public static void EnsureLoginAllowed(UserStatus status, bool isDeleted, bool passwordIsValid)
    {
        if (isDeleted || !passwordIsValid)
        {
            throw new AuthenticationFailedException("Invalid login credentials.");
        }

        if (status != UserStatus.Active)
        {
            throw new AuthorizationDeniedException("This account is not active.");
        }
    }
}
