using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Coupons.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Coupons.DTOs.Responses;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Coupons.Interfaces;
public interface IValidateCouponService
{
    Task<CouponValidationResponse> ValidateAsync(ValidateCouponRequest request, CancellationToken cancellationToken);
}
