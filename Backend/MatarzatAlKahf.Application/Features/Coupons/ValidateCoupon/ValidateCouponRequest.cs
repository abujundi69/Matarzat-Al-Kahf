namespace MatarzatAlKahf.Application.Features.Coupons.DTOs.Requests;

public sealed record ValidateCouponRequest(string Code, string? GuestCartToken);
