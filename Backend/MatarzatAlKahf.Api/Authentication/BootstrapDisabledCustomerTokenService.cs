using MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;

namespace MatarzatAlKahf.Api.Authentication;

internal sealed class BootstrapDisabledCustomerTokenService : ICustomerTokenService
{
    public CustomerAccessToken CreateAccessToken(CustomerTokenDescriptor descriptor) =>
        throw new InvalidOperationException(
            "Customer token creation is unavailable during the bootstrap command.");
}
