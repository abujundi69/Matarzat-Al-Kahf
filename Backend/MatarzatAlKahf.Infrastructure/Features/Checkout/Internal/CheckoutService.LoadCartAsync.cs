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
    private async Task<Cart> LoadCartAsync(string? guestToken, CancellationToken cancellationToken)
    {
        IQueryable<Cart> query = dbContext.Carts.Include(cart => cart.Coupon).ThenInclude(coupon => coupon!.Categories).Include(cart => cart.Coupon).ThenInclude(coupon => coupon!.Products).Include(cart => cart.Items).ThenInclude(item => item.ProductVariant).ThenInclude(variant => variant.InventoryItem).Include(cart => cart.Items).ThenInclude(item => item.ProductVariant).ThenInclude(variant => variant.Product).ThenInclude(product => product.Translations);
        Cart? cart;
        if (currentActor.CustomerId.HasValue)
        {
            cart = await query.SingleOrDefaultAsync(item => item.CustomerId == currentActor.CustomerId && item.Status == CartStatus.Active, cancellationToken);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(guestToken))
            {
                throw ValidationException.For(nameof(guestToken), "Guest cart token is required.");
            }

            string hash = Hash(guestToken);
            cart = await query.SingleOrDefaultAsync(item => item.GuestTokenHash == hash && item.CustomerId == null && item.Status == CartStatus.Active, cancellationToken);
        }

        if (cart is null || cart.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new NotFoundException("Cart was not found.");
        }

        return cart;
    }
}
