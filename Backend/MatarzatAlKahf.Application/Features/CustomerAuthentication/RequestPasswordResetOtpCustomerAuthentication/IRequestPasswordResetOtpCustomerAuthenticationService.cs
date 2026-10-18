using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Requests;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
public interface IRequestPasswordResetOtpCustomerAuthenticationService
{
    Task<OtpRequestResponse> RequestPasswordResetOtpAsync(RequestOtpRequest request, CancellationToken cancellationToken);
}
