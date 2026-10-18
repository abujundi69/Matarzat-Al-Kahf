using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Coupons.DTOs.Requests;

public sealed record SaveCouponRequest(
    string Code,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MinimumOrderAmount,
    decimal? MaximumDiscountAmount,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc,
    int? TotalUsageLimit,
    int? UsageLimitPerCustomer,
    CouponStatus Status,
    IReadOnlyList<Guid> IncludedCategoryIds,
    IReadOnlyList<Guid> IncludedProductIds,
    IReadOnlyList<Guid> ExcludedProductIds);
