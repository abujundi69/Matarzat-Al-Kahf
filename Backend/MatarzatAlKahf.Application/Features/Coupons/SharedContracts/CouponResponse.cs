using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Coupons.DTOs.Responses;
public sealed record CouponResponse(Guid Id, string Code, DiscountType DiscountType, decimal DiscountValue, decimal? MinimumOrderAmount, decimal? MaximumDiscountAmount, DateTime? StartsAtUtc, DateTime? EndsAtUtc, int? TotalUsageLimit, int UsageCount, int? UsageLimitPerCustomer, CouponStatus Status, IReadOnlyList<Guid> IncludedCategoryIds, IReadOnlyList<Guid> IncludedProductIds, IReadOnlyList<Guid> ExcludedProductIds);
