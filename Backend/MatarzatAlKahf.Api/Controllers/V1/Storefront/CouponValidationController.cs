using MatarzatAlKahf.Application.Features.Coupons.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Coupons.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Coupons.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Storefront;
[ApiController]
[Route("api/v1/coupons")]
[Tags("Coupons")]
public sealed class CouponValidationController(IValidateCouponService validateCouponService) : ControllerBase
{
    [HttpPost("validate")]
    public async Task<ActionResult<CouponValidationResponse>> Validate(ValidateCouponRequest request, CancellationToken cancellationToken) => Ok(await validateCouponService.ValidateAsync(request, cancellationToken));
}
