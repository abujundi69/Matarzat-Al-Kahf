using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
public interface ICustomerTokenService
{
    CustomerAccessToken CreateAccessToken(CustomerTokenDescriptor descriptor);
}
