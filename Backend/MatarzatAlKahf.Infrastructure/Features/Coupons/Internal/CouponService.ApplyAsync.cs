using System.Security.Cryptography;
using System.Text;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Coupons.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Coupons.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Coupons.Interfaces;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Entities.Promotions;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Coupons.Services;
public sealed partial class CouponService
{
    private async Task ApplyAsync(Coupon coupon, SaveCouponRequest request, CancellationToken cancellationToken)
    {
        Guid[] categoryIds = request.IncludedCategoryIds.Distinct().ToArray();
        Guid[] includedIds = request.IncludedProductIds.Distinct().ToArray();
        Guid[] excludedIds = request.ExcludedProductIds.Distinct().ToArray();
        Guid[] allProductIds = includedIds.Concat(excludedIds).ToArray();
        if (includedIds.Intersect(excludedIds).Any())
        {
            throw ValidationException.For(nameof(request.IncludedProductIds), "A product cannot be both included and excluded.");
        }

        int categories = await dbContext.Categories.CountAsync(item => categoryIds.Contains(item.Id) && !item.IsDeleted, cancellationToken);
        int products = await dbContext.Products.CountAsync(item => allProductIds.Contains(item.Id) && !item.IsDeleted, cancellationToken);
        if (categories != categoryIds.Length || products != includedIds.Length + excludedIds.Length)
        {
            throw ValidationException.For(nameof(request), "One or more scoped resources do not exist.");
        }

        coupon.Code = request.Code.Trim().ToUpperInvariant();
        coupon.DiscountType = request.DiscountType;
        coupon.DiscountValue = request.DiscountValue;
        coupon.MinimumOrderAmount = request.MinimumOrderAmount;
        coupon.MaximumDiscountAmount = request.MaximumDiscountAmount;
        coupon.StartsAtUtc = request.StartsAtUtc;
        coupon.EndsAtUtc = request.EndsAtUtc;
        coupon.TotalUsageLimit = request.TotalUsageLimit;
        coupon.UsageLimitPerCustomer = request.UsageLimitPerCustomer;
        coupon.Status = request.Status;
        foreach (Guid categoryId in categoryIds)
        {
            coupon.Categories.Add(new CouponCategory { CouponId = coupon.Id, CategoryId = categoryId });
        }

        foreach (Guid productId in includedIds)
        {
            coupon.Products.Add(new CouponProduct { CouponId = coupon.Id, ProductId = productId });
        }

        foreach (Guid productId in excludedIds)
        {
            coupon.Products.Add(new CouponProduct { CouponId = coupon.Id, ProductId = productId, IsExcluded = true });
        }
    }
}
