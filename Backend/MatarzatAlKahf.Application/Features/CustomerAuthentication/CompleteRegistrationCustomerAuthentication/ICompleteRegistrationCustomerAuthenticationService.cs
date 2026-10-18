using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Requests;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
public interface ICompleteRegistrationCustomerAuthenticationService
{
    Task<CustomerLoginResponse> CompleteRegistrationAsync(CompleteRegistrationRequest request, CancellationToken cancellationToken);
}
