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
    private CartResponse Map(Cart cart, string? token)
    {
        LanguageCode language = languageResolver.GetCurrentLanguage();
        CartItemResponse[] items = cart.Items.Where(item => !item.IsDeleted).Select(item =>
        {
            var variant = item.ProductVariant;
            decimal unitPrice = variant.PriceOverride ?? variant.Product.BasePrice;
            string name = variant.Product.Translations.FirstOrDefault(translation => translation.Language == language)?.Name ?? variant.Product.Translations.FirstOrDefault(translation => translation.Language == LanguageCode.Arabic)?.Name ?? variant.Product.Translations.FirstOrDefault()?.Name ?? variant.Sku;
            return new CartItemResponse(item.Id, variant.ProductId, variant.Id, name, variant.Sku, variant.Size, variant.Color, unitPrice, item.Quantity, unitPrice * item.Quantity, variant.InventoryItem.QuantityOnHand - variant.InventoryItem.ReservedQuantity);
        }).OrderBy(item => item.Name).ToArray();
        decimal subtotal = items.Sum(item => item.Total);
        decimal discount = cart.Coupon is null ? 0 : CalculateDiscount(cart.Coupon, subtotal);
        CurrencyCode currency = cart.Items.FirstOrDefault()?.ProductVariant.Product.Currency ?? CurrencyCode.ILS;
        return new CartResponse(cart.Id, token, items, cart.Coupon?.Code, subtotal, discount, subtotal - discount, currency, cart.ExpiresAtUtc, Convert.ToBase64String(cart.RowVersion));
    }
}
