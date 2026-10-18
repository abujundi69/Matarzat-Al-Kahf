using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Authentication.Admin;
public interface IAdminAuthenticationService
{
    Task<AdminLoginResponse> LoginAsync(AdminLoginRequest request, CancellationToken cancellationToken);
}
