using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Requests;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MatarzatAlKahf.Api.Controllers.V1.Customer;
[ApiController]
[Route("api/v1/customer/auth")]
[Tags("Customer Authentication")]
public sealed class CustomerAuthenticationController(IRequestRegistrationOtpCustomerAuthenticationService requestRegistrationOtpCustomerAuthenticationService, IVerifyRegistrationOtpCustomerAuthenticationService verifyRegistrationOtpCustomerAuthenticationService, ICompleteRegistrationCustomerAuthenticationService completeRegistrationCustomerAuthenticationService, IRequestPasswordResetOtpCustomerAuthenticationService requestPasswordResetOtpCustomerAuthenticationService, IVerifyPasswordResetOtpCustomerAuthenticationService verifyPasswordResetOtpCustomerAuthenticationService, IResetPasswordCustomerAuthenticationService resetPasswordCustomerAuthenticationService, ILogoutCustomerAuthenticationService logoutCustomerAuthenticationService) : ControllerBase
{
    [HttpPost("register/request-otp")]
    [EnableRateLimiting("Otp")]
    public async Task<ActionResult<OtpRequestResponse>> RequestRegistrationOtp(RequestOtpRequest request, CancellationToken cancellationToken) => Ok(await requestRegistrationOtpCustomerAuthenticationService.RequestRegistrationOtpAsync(request, cancellationToken));
    [HttpPost("register/verify-otp")]
    [EnableRateLimiting("Otp")]
    public async Task<ActionResult<OtpVerificationResponse>> VerifyRegistrationOtp(VerifyOtpRequest request, CancellationToken cancellationToken) => Ok(await verifyRegistrationOtpCustomerAuthenticationService.VerifyRegistrationOtpAsync(request, cancellationToken));
    [HttpPost("register/complete")]
    [EnableRateLimiting("CustomerAuthentication")]
    public async Task<ActionResult<CustomerLoginResponse>> CompleteRegistration(CompleteRegistrationRequest request, CancellationToken cancellationToken) => Ok(await completeRegistrationCustomerAuthenticationService.CompleteRegistrationAsync(request, cancellationToken));
    [HttpPost("forgot-password/request-otp")]
    [EnableRateLimiting("Otp")]
    public async Task<ActionResult<OtpRequestResponse>> RequestPasswordResetOtp(RequestOtpRequest request, CancellationToken cancellationToken) => Ok(await requestPasswordResetOtpCustomerAuthenticationService.RequestPasswordResetOtpAsync(request, cancellationToken));
    [HttpPost("forgot-password/verify-otp")]
    [EnableRateLimiting("Otp")]
    public async Task<ActionResult<OtpVerificationResponse>> VerifyPasswordResetOtp(VerifyOtpRequest request, CancellationToken cancellationToken) => Ok(await verifyPasswordResetOtpCustomerAuthenticationService.VerifyPasswordResetOtpAsync(request, cancellationToken));
    [HttpPost("reset-password")]
    [EnableRateLimiting("CustomerAuthentication")]
    public async Task<IActionResult> ResetPassword(ResetCustomerPasswordRequest request, CancellationToken cancellationToken)
    {
        await resetPasswordCustomerAuthenticationService.ResetPasswordAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("logout")]
    [Authorize(Policy = AuthenticationPolicyNames.Customer)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        string tokenId = User.FindFirstValue(JwtRegisteredClaimNames.Jti) ?? string.Empty;
        await logoutCustomerAuthenticationService.LogoutAsync(tokenId, cancellationToken);
        return NoContent();
    }
}
