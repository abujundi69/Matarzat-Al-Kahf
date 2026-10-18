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
    private async Task<Cart> LoadCartAsync(string? token, CancellationToken cancellationToken)
    {
        IQueryable<Cart> query = dbContext.Carts.AsNoTracking().Include(cart => cart.Items).ThenInclude(item => item.ProductVariant).ThenInclude(variant => variant.Product);
        Cart? cart;
        if (currentActor.CustomerId.HasValue)
        {
            cart = await query.SingleOrDefaultAsync(item => item.CustomerId == currentActor.CustomerId && item.Status == CartStatus.Active, cancellationToken);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw ValidationException.For(nameof(token), "Guest cart token is required.");
            }

            string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
            cart = await query.SingleOrDefaultAsync(item => item.GuestTokenHash == hash && item.Status == CartStatus.Active, cancellationToken);
        }

        return cart is null || cart.ExpiresAtUtc <= DateTime.UtcNow ? throw new NotFoundException("Cart was not found.") : cart;
    }
}
