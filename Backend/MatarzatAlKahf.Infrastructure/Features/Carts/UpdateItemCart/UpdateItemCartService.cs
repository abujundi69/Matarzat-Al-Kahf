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
    public async Task<CartResponse> UpdateItemAsync(string? guestToken, Guid itemId, UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        ValidateQuantity(request.Quantity);
        (Cart cart, string? token) = await GetOrCreateAsync(guestToken, cancellationToken);
        CartItem item = cart.Items.SingleOrDefault(candidate => candidate.Id == itemId) ?? throw new NotFoundException("Cart item was not found.");
        int available = item.ProductVariant.InventoryItem.QuantityOnHand - item.ProductVariant.InventoryItem.ReservedQuantity;
        EnsureStock(available, request.Quantity);
        item.Quantity = request.Quantity;
        item.UpdatedAtUtc = DateTime.UtcNow;
        Touch(cart);
        await SaveAsync(cancellationToken);
        return Map(cart, token);
    }
}
