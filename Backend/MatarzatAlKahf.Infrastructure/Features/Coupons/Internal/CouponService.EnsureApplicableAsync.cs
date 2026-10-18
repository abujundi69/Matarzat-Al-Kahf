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
    private async Task EnsureApplicableAsync(Coupon coupon, decimal subtotal, CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;
        if (coupon.Status != CouponStatus.Active || (coupon.StartsAtUtc.HasValue && coupon.StartsAtUtc > now) || (coupon.EndsAtUtc.HasValue && coupon.EndsAtUtc <= now) || (coupon.MinimumOrderAmount.HasValue && subtotal < coupon.MinimumOrderAmount))
        {
            throw new ValidationException("Coupon is not applicable.", new Dictionary<string, string[]>());
        }

        if (coupon.TotalUsageLimit.HasValue && await dbContext.CouponUsages.CountAsync(usage => usage.CouponId == coupon.Id, cancellationToken) >= coupon.TotalUsageLimit)
        {
            throw new ConflictException("Coupon usage limit has been reached.");
        }

        if (currentActor.CustomerId.HasValue && coupon.UsageLimitPerCustomer.HasValue && await dbContext.CouponUsages.CountAsync(usage => usage.CouponId == coupon.Id && usage.CustomerId == currentActor.CustomerId, cancellationToken) >= coupon.UsageLimitPerCustomer)
        {
            throw new ConflictException("Coupon customer usage limit has been reached.");
        }
    }
}
