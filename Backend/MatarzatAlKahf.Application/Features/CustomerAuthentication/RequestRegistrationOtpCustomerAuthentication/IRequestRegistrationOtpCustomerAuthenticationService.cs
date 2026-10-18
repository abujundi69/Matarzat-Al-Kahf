using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Requests;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
public interface IRequestRegistrationOtpCustomerAuthenticationService
{
    Task<OtpRequestResponse> RequestRegistrationOtpAsync(RequestOtpRequest request, CancellationToken cancellationToken);
}
