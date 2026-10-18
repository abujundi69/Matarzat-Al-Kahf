using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Coupons.DTOs.Responses;
public sealed record CouponValidationResponse(string Code, decimal Subtotal, decimal Discount, decimal Total);
