using System.Data;
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
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Carts.Services;

public sealed partial class CartService
{
    public async Task<CartResponse> MergeAsync(MergeCartRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Guid customerId = currentActor.CustomerId ?? throw new AuthenticationFailedException("Customer authentication is required.");
        if (string.IsNullOrWhiteSpace(request.GuestCartToken))
        {
            throw ValidationException.For(nameof(request.GuestCartToken), "Guest cart token is required.");
        }

        string hash = HashToken(request.GuestCartToken);
        return await DbTransactionExecutor.ExecuteAsync(dbContext, async token =>
        {
            await AcquireCartLockAsync($"customer:{customerId:N}", token);
            await AcquireCartLockAsync($"guest:{hash}", token);
            // A guest token that matches no mergeable cart (expired, purged, or issued by a
            // database that no longer exists) leaves nothing to merge, which is the same outcome
            // as a cart that was already merged.
            Cart? guest = await Query().SingleOrDefaultAsync(
                cart => cart.GuestTokenHash == hash
                    && ((cart.CustomerId == null
                            && cart.Status == CartStatus.Active
                            && cart.ExpiresAtUtc > DateTime.UtcNow)
                        || (cart.CustomerId == customerId
                            && cart.Status == CartStatus.Converted)),
                token);
            Cart? customer = await Query().SingleOrDefaultAsync(
                cart => cart.CustomerId == customerId
                    && cart.Status == CartStatus.Active,
                token);

            if (guest is null || guest.Status == CartStatus.Converted)
            {
                if (customer is null)
                {
                    customer = new Cart
                    {
                        CustomerId = customerId,
                        GuestTokenHash = string.Empty,
                        Status = CartStatus.Active,
                        ExpiresAtUtc = DateTime.UtcNow.Add(CartLifetime)
                    };
                    dbContext.Carts.Add(customer);
                    await dbContext.SaveChangesAsync(token);
                }

                return Map(await ReloadAsync(customer.Id, token), null);
            }

            if (customer is null)
            {
                customer = new Cart
                {
                    CustomerId = customerId,
                    GuestTokenHash = string.Empty,
                    Status = CartStatus.Active,
                    ExpiresAtUtc = DateTime.UtcNow.Add(CartLifetime)
                };
                dbContext.Carts.Add(customer);
            }

            foreach (CartItem guestItem in guest.Items.Where(item => IsPurchasable(item.ProductVariant)))
            {
                CartItem? target = customer.Items.SingleOrDefault(
                    item => !item.IsDeleted
                        && item.ProductVariantId == guestItem.ProductVariantId);
                int quantity = (target?.Quantity ?? 0) + guestItem.Quantity;
                int available = guestItem.ProductVariant.InventoryItem.QuantityOnHand
                    - guestItem.ProductVariant.InventoryItem.ReservedQuantity;
                EnsureStock(available, quantity);
                if (target is null)
                {
                    var item = new CartItem
                    {
                        CartId = customer.Id,
                        Cart = customer,
                        ProductVariantId = guestItem.ProductVariantId,
                        ProductVariant = guestItem.ProductVariant,
                        Quantity = quantity
                    };
                    customer.Items.Add(item);
                    dbContext.CartItems.Add(item);
                }
                else
                {
                    target.Quantity = quantity;
                    target.UpdatedAtUtc = DateTime.UtcNow;
                }
            }

            customer.CouponId ??= guest.CouponId;
            guest.CustomerId = customerId;
            guest.Status = CartStatus.Converted;
            guest.UpdatedAtUtc = DateTime.UtcNow;
            Touch(customer);
            await SaveAsync(token);
            return Map(await ReloadAsync(customer.Id, token), null);
        }, cancellationToken, IsolationLevel.Serializable);
    }
}
