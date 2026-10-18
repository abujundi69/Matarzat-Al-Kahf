using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Checkout.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Checkout.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Checkout.Interfaces;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;
using MatarzatAlKahf.Application.Features.Notifications.Interfaces;
using MatarzatAlKahf.Application.Features.Payments.Interfaces;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Entities.Customers;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Entities.Promotions;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Checkout.Services;
public sealed partial class CheckoutService
{
    private async Task<decimal> CalculateCouponDiscountAsync(Cart cart, decimal subtotal, CancellationToken cancellationToken)
    {
        Coupon? coupon = cart.Coupon;
        if (coupon is null)
        {
            return 0;
        }

        DateTime now = DateTime.UtcNow;
        if (coupon.Status != CouponStatus.Active || coupon.IsDeleted || (coupon.StartsAtUtc.HasValue && coupon.StartsAtUtc > now) || (coupon.EndsAtUtc.HasValue && coupon.EndsAtUtc <= now) || (coupon.MinimumOrderAmount.HasValue && subtotal < coupon.MinimumOrderAmount))
        {
            throw new ConflictException("The applied coupon is no longer valid.");
        }

        int totalUsages = await dbContext.CouponUsages.CountAsync(usage => usage.CouponId == coupon.Id, cancellationToken);
        if (coupon.TotalUsageLimit.HasValue && totalUsages >= coupon.TotalUsageLimit)
        {
            throw new ConflictException("The coupon usage limit has been reached.");
        }

        if (currentActor.CustomerId.HasValue && coupon.UsageLimitPerCustomer.HasValue)
        {
            int customerUsages = await dbContext.CouponUsages.CountAsync(usage => usage.CouponId == coupon.Id && usage.CustomerId == currentActor.CustomerId, cancellationToken);
            if (customerUsages >= coupon.UsageLimitPerCustomer)
            {
                throw new ConflictException("The coupon customer usage limit has been reached.");
            }
        }

        decimal eligibleSubtotal = cart.Items.Where(item => IsEligible(coupon, item.ProductVariant.Product)).Sum(item => (item.ProductVariant.PriceOverride ?? item.ProductVariant.Product.BasePrice) * item.Quantity);
        decimal discount = coupon.DiscountType == DiscountType.Percentage ? eligibleSubtotal * coupon.DiscountValue / 100 : Math.Min(coupon.DiscountValue, eligibleSubtotal);
        if (coupon.MaximumDiscountAmount.HasValue)
        {
            discount = Math.Min(discount, coupon.MaximumDiscountAmount.Value);
        }

        return decimal.Round(discount, 2);
    }
}
