using MatarzatAlKahf.Application.Features.Checkout.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Checkout.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Checkout.Interfaces;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Requests;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MatarzatAlKahf.Api.Controllers.V1.Storefront;
[ApiController]
[Route("api/v1/checkout")]
[AllowAnonymous]
[Tags("Checkout")]
public sealed class CheckoutController(IPreviewCheckoutService previewCheckoutService, IPlaceOrderCheckoutService placeOrderCheckoutService, IRequestGuestCheckoutOtpCustomerAuthenticationService requestGuestCheckoutOtpCustomerAuthenticationService, IVerifyGuestCheckoutOtpCustomerAuthenticationService verifyGuestCheckoutOtpCustomerAuthenticationService, ICurrentActor currentActor) : ControllerBase
{
    [HttpPost("guest/request-otp")]
    [EnableRateLimiting("Otp")]
    public async Task<ActionResult<OtpRequestResponse>> RequestGuestOtp(RequestOtpRequest request, CancellationToken cancellationToken) => Ok(await requestGuestCheckoutOtpCustomerAuthenticationService.RequestGuestCheckoutOtpAsync(request, cancellationToken));
    [HttpPost("guest/verify-otp")]
    [EnableRateLimiting("Otp")]
    public async Task<ActionResult<OtpVerificationResponse>> VerifyGuestOtp(VerifyOtpRequest request, CancellationToken cancellationToken) => Ok(await verifyGuestCheckoutOtpCustomerAuthenticationService.VerifyGuestCheckoutOtpAsync(request, cancellationToken));
    [HttpPost("preview")]
    public async Task<ActionResult<CheckoutPreviewResponse>> Preview(CheckoutRequest request, CancellationToken cancellationToken) => Ok(await previewCheckoutService.PreviewAsync(request, cancellationToken));
    [HttpPost("orders")]
    public async Task<ActionResult<PlaceOrderResponse>> PlaceOrder(CheckoutRequest request, [FromHeader(Name = "Idempotency-Key")] string idempotencyKey, CancellationToken cancellationToken)
    {
        PlaceOrderResponse response = await placeOrderCheckoutService.PlaceOrderAsync(request, idempotencyKey, cancellationToken);
        if (response.IsDuplicate)
        {
            return Ok(response);
        }

        string location = currentActor.CustomerId.HasValue
            ? $"/api/v1/customer/orders/{response.OrderId}"
            : "/api/v1/orders/lookup";
        return Created(location, response);
    }
}
