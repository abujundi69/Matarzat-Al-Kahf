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
    public async Task<CartResponse> AddItemAsync(string? guestToken, AddCartItemRequest request, CancellationToken cancellationToken)
    {
        ValidateQuantity(request.Quantity);
        (Cart cart, string? token) = await GetOrCreateAsync(guestToken, cancellationToken);
        var variant = await dbContext.ProductVariants.Include(item => item.Product).Include(item => item.InventoryItem).SingleOrDefaultAsync(item => item.Id == request.ProductVariantId && !item.IsDeleted && item.IsActive && !item.Product.IsDeleted && item.Product.Status == ProductStatus.Active, cancellationToken) ?? throw new NotFoundException("Product variant was not found.");
        CartItem? existing = cart.Items.SingleOrDefault(item => item.ProductVariantId == request.ProductVariantId);
        int targetQuantity = (existing?.Quantity ?? 0) + request.Quantity;
        EnsureStock(variant.InventoryItem.QuantityOnHand - variant.InventoryItem.ReservedQuantity, targetQuantity);
        EnsureCurrency(cart, variant.Product.Currency);
        if (existing is null)
        {
            var item = new CartItem
            {
                CartId = cart.Id,
                Cart = cart,
                ProductVariantId = variant.Id,
                ProductVariant = variant,
                Quantity = request.Quantity
            };
            cart.Items.Add(item);
            dbContext.CartItems.Add(item);
        }
        else
        {
            existing.Quantity = targetQuantity;
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }

        Touch(cart);
        await SaveAsync(cancellationToken);
        return Map(await ReloadAsync(cart.Id, cancellationToken), token);
    }
}
