using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Infrastructure.Features.Carts.Services;

public sealed partial class CartService
{
    /// <summary>
    /// Drops cart lines whose product or variant is no longer purchasable. Without this a
    /// discontinued, hidden, or deleted product stays visible in the cart, inflates the totals,
    /// and makes checkout fail with an unrecoverable conflict the shopper cannot clear.
    /// </summary>
    private async Task PruneUnavailableItemsAsync(Cart cart, CancellationToken cancellationToken)
    {
        CartItem[] unavailable = cart.Items
            .Where(item => !IsPurchasable(item.ProductVariant))
            .ToArray();
        if (unavailable.Length == 0)
        {
            return;
        }

        foreach (CartItem item in unavailable)
        {
            dbContext.CartItems.Remove(item);
            cart.Items.Remove(item);
        }

        cart.UpdatedAtUtc = DateTime.UtcNow;
        await SaveAsync(cancellationToken);
    }

    /// <summary>
    /// Mirrors the availability rule checkout enforces, so the cart never shows a line that
    /// checkout would reject.
    /// </summary>
    private static bool IsPurchasable(ProductVariant variant) =>
        !variant.IsDeleted
        && variant.IsActive
        && !variant.Product.IsDeleted
        && variant.Product.Status == ProductStatus.Active;
}
