using System.Security.Cryptography;
using System.Text;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Carts.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Carts.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Carts.Interfaces;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Entities.Promotions;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Carts.Services;
public sealed partial class CartService
{
    private async Task ValidateCouponAsync(Coupon coupon, decimal subtotal, CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;
        if (coupon.Status != CouponStatus.Active || (coupon.StartsAtUtc.HasValue && coupon.StartsAtUtc > now) || (coupon.EndsAtUtc.HasValue && coupon.EndsAtUtc <= now) || (coupon.MinimumOrderAmount.HasValue && subtotal < coupon.MinimumOrderAmount))
        {
            throw new ValidationException("Coupon is not applicable to this cart.", new Dictionary<string, string[]>());
        }

        if (coupon.TotalUsageLimit.HasValue && await dbContext.CouponUsages.CountAsync(usage => usage.CouponId == coupon.Id, cancellationToken) >= coupon.TotalUsageLimit)
        {
            throw new ValidationException("Coupon usage limit has been reached.", new Dictionary<string, string[]>());
        }

        if (currentActor.CustomerId.HasValue && coupon.UsageLimitPerCustomer.HasValue && await dbContext.CouponUsages.CountAsync(usage => usage.CouponId == coupon.Id && usage.CustomerId == currentActor.CustomerId, cancellationToken) >= coupon.UsageLimitPerCustomer)
        {
            throw new ValidationException("Coupon usage limit has been reached.", new Dictionary<string, string[]>());
        }
    }
}
