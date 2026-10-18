using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Coupons.DTOs.Requests;

public sealed record CouponQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    CouponStatus? Status = null);
