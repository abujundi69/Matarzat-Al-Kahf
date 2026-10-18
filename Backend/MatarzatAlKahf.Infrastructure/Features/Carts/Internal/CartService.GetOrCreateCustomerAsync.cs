using System.Data;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Carts.Services;

public sealed partial class CartService
{
    private Task<Cart> GetOrCreateCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken) =>
        DbTransactionExecutor.ExecuteAsync(dbContext, async token =>
        {
            await AcquireCartLockAsync($"customer:{customerId:N}", token);
            Cart? cart = await Query().SingleOrDefaultAsync(
                candidate => candidate.CustomerId == customerId
                    && candidate.Status == CartStatus.Active,
                token);
            if (cart is not null && cart.ExpiresAtUtc <= DateTime.UtcNow)
            {
                cart.Status = CartStatus.Expired;
                cart.UpdatedAtUtc = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(token);
                cart = null;
            }

            if (cart is null)
            {
                cart = new Cart
                {
                    CustomerId = customerId,
                    GuestTokenHash = string.Empty,
                    Status = CartStatus.Active,
                    ExpiresAtUtc = DateTime.UtcNow.Add(CartLifetime)
                };
                dbContext.Carts.Add(cart);
                await dbContext.SaveChangesAsync(token);
                await dbContext.Entry(cart).ReloadAsync(token);
            }

            return cart;
        }, cancellationToken, IsolationLevel.Serializable);
}
