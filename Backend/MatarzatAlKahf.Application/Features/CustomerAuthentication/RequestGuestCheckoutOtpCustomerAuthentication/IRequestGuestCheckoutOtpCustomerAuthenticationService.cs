using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Requests;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
public interface IRequestGuestCheckoutOtpCustomerAuthenticationService
{
    Task<OtpRequestResponse> RequestGuestCheckoutOtpAsync(RequestOtpRequest request, CancellationToken cancellationToken);
}
