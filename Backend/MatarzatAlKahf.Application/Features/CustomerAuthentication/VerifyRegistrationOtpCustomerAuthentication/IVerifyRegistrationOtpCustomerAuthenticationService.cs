using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Requests;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
public interface IVerifyRegistrationOtpCustomerAuthenticationService
{
    Task<OtpVerificationResponse> VerifyRegistrationOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken);
}
