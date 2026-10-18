using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Requests;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
public interface IVerifyGuestCheckoutOtpCustomerAuthenticationService
{
    Task<OtpVerificationResponse> VerifyGuestCheckoutOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken);
}
