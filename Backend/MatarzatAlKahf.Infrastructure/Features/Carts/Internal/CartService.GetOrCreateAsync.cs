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
    private async Task<(Cart Cart, string? Token)> GetOrCreateAsync(string? guestToken, CancellationToken cancellationToken)
    {
        Guid? customerId = currentActor.CustomerId;
        Cart? cart = null;
        string? token = null;
        if (customerId.HasValue)
        {
            cart = await GetOrCreateCustomerAsync(customerId.Value, cancellationToken);
            await PruneUnavailableItemsAsync(cart, cancellationToken);
            return (cart, null);
        }

        // A token that matches no active guest cart (converted at checkout, expired, or issued
        // by a database that no longer exists) is not an error: the visitor simply has no cart
        // yet, so start a new one and hand back its token.
        if (!string.IsNullOrWhiteSpace(guestToken))
        {
            string hash = HashToken(guestToken);
            cart = await Query().SingleOrDefaultAsync(candidate => candidate.GuestTokenHash == hash && candidate.CustomerId == null && candidate.Status == CartStatus.Active, cancellationToken);
        }

        if (cart is not null && cart.ExpiresAtUtc <= DateTime.UtcNow)
        {
            cart.Status = CartStatus.Expired;
            await dbContext.SaveChangesAsync(cancellationToken);
            cart = null;
        }

        if (cart is null)
        {
            token = customerId.HasValue ? null : CreateToken();
            cart = new Cart
            {
                CustomerId = customerId,
                GuestTokenHash = token is null ? string.Empty : HashToken(token),
                Status = CartStatus.Active,
                ExpiresAtUtc = DateTime.UtcNow.Add(CartLifetime)
            };
            dbContext.Carts.Add(cart);
            await dbContext.SaveChangesAsync(cancellationToken);
            await dbContext.Entry(cart).ReloadAsync(cancellationToken);
        }

        await PruneUnavailableItemsAsync(cart, cancellationToken);
        return (cart, token ?? guestToken);
    }
}
