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
    public async Task<CartResponse> ApplyCouponAsync(string? guestToken, ApplyCartCouponRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            throw ValidationException.For(nameof(request.Code), "Coupon code is required.");
        }

        (Cart cart, string? token) = await GetOrCreateAsync(guestToken, cancellationToken);
        string normalizedCode = request.Code.Trim().ToUpperInvariant();
        Coupon coupon = await dbContext.Coupons.SingleOrDefaultAsync(candidate => candidate.Code == normalizedCode && !candidate.IsDeleted, cancellationToken) ?? throw new ValidationException("Coupon is invalid.", new Dictionary<string, string[]>());
        await ValidateCouponAsync(coupon, CalculateSubtotal(cart), cancellationToken);
        cart.CouponId = coupon.Id;
        cart.Coupon = coupon;
        Touch(cart);
        await SaveAsync(cancellationToken);
        return Map(cart, token);
    }
}
