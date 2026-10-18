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
    public async Task<CouponValidationResponse> ValidateAsync(ValidateCouponRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            throw ValidationException.For(nameof(request.Code), "Coupon code is required.");
        }

        Cart cart = await LoadCartAsync(request.GuestCartToken, cancellationToken);
        string normalizedCode = request.Code.Trim().ToUpperInvariant();
        Coupon coupon = await Query().AsNoTracking().SingleOrDefaultAsync(item => item.Code == normalizedCode && !item.IsDeleted, cancellationToken) ?? throw ValidationException.For(nameof(request.Code), "Coupon is invalid.");
        decimal subtotal = cart.Items.Sum(item => (item.ProductVariant.PriceOverride ?? item.ProductVariant.Product.BasePrice) * item.Quantity);
        await EnsureApplicableAsync(coupon, subtotal, cancellationToken);
        decimal eligible = cart.Items.Where(item => IsEligible(coupon, item)).Sum(item => (item.ProductVariant.PriceOverride ?? item.ProductVariant.Product.BasePrice) * item.Quantity);
        decimal discount = CalculateDiscount(coupon, eligible);
        return new CouponValidationResponse(coupon.Code, subtotal, discount, subtotal - discount);
    }
}
