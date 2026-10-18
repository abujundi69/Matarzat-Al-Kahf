using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Requests;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
public interface IVerifyPasswordResetOtpCustomerAuthenticationService
{
    Task<OtpVerificationResponse> VerifyPasswordResetOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken);
}
