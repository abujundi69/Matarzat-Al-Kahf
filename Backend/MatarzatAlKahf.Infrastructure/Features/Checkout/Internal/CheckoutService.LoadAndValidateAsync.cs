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
    private async Task<CheckoutState> LoadAndValidateAsync(CheckoutRequest request, bool consumeGuestVerification, CancellationToken cancellationToken)
    {
        ValidateRequest(request);
        Cart cart = await LoadCartAsync(request.GuestCartToken, cancellationToken);
        if (cart.Items.Count == 0)
        {
            throw new ConflictException("The cart is empty.");
        }

        DeliveryLocation location = await dbContext.DeliveryLocations.AsNoTracking().SingleOrDefaultAsync(item => item.Id == request.DeliveryLocationId && item.IsActive && !item.IsDeleted && (item.CountryCode == "PS" || item.CountryCode == "ps" || item.CountryCode == "Ps" || item.CountryCode == "pS"), cancellationToken) ?? throw ValidationException.For(nameof(request.DeliveryLocationId), "Delivery location is not supported.");
        if (request.PaymentMethod == PaymentMethod.CashOnDelivery && !location.CashOnDeliveryAllowed)
        {
            throw new ConflictException("Cash on delivery is not available for this location.");
        }

        AddressValues address = await ResolveAddressAsync(request, cancellationToken);
        if (!currentActor.CustomerId.HasValue)
        {
            await ValidateGuestPhoneAsync(address.PhoneNumber, request.GuestPhoneVerificationToken, consumeGuestVerification, cancellationToken);
        }

        foreach (CartItem item in cart.Items)
        {
            ProductVariant variant = item.ProductVariant;
            if (variant.IsDeleted || !variant.IsActive || variant.Product.IsDeleted || variant.Product.Status != ProductStatus.Active)
            {
                throw new ConflictException($"SKU '{variant.Sku}' is no longer available.");
            }

            int available = variant.InventoryItem.QuantityOnHand - variant.InventoryItem.ReservedQuantity;
            if (item.Quantity > available)
            {
                throw new ConflictException($"Insufficient stock for SKU '{variant.Sku}'.");
            }
        }

        CurrencyCode[] currencies = cart.Items.Select(item => item.ProductVariant.Product.Currency).Distinct().ToArray();
        if (currencies.Length != 1 || currencies[0] != CurrencyCode.ILS)
        {
            throw new ConflictException("Checkout supports ILS only.");
        }

        decimal subtotal = cart.Items.Sum(item => (item.ProductVariant.PriceOverride ?? item.ProductVariant.Product.BasePrice) * item.Quantity);
        decimal discount = await CalculateCouponDiscountAsync(cart, subtotal, cancellationToken);
        decimal delivery = location.AdditionalDeliveryCost;
        decimal total = subtotal - discount + delivery;
        if (location.MinimumOrderAmount.HasValue && total < location.MinimumOrderAmount)
        {
            throw new ConflictException("The order does not meet the delivery location minimum.");
        }

        return new CheckoutState(cart, location, address.CustomerName, address.PhoneNumber, address.Area, address.AddressLine, address.TaxNumber, address.DeliveryNotes, subtotal, discount, delivery, total, currencies[0]);
    }
}
