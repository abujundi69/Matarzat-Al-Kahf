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
    private async Task<PlaceOrderResponse> PlaceCoreAsync(CheckoutRequest request, string idempotencyKey, string scope, string requestHash, CancellationToken cancellationToken)
    {
        CheckoutIdempotency? existing = await dbContext.CheckoutIdempotencies.Include(record => record.Order).ThenInclude(order => order!.Payments).SingleOrDefaultAsync(record => record.Scope == scope && record.Key == idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            if (existing.RequestHash != requestHash)
            {
                throw new ConflictException("The idempotency key was already used for a different checkout request.");
            }

            if (existing.Order is null)
            {
                throw new ConflictException("The checkout request is already being processed.");
            }

            Payment existingPayment = existing.Order.Payments
                .Where(item => !item.IsDeleted)
                .OrderByDescending(item => item.CreatedAtUtc)
                .FirstOrDefault()
                ?? throw new ConflictException("The existing checkout does not have a valid payment record.");
            return MapOrder(existing.Order, existingPayment, true, null);
        }

        await paymentMethodService.EnsureEnabledAsync(request.PaymentMethod, cancellationToken);
        if (IsOnline(request.PaymentMethod)
            && !onlinePaymentGateway.IsConfigured)
        {
            throw new ServiceUnavailableException(
                "Online payment is unavailable because no verified gateway is configured.");
        }

        CheckoutState state = await LoadAndValidateAsync(request, true, cancellationToken);
        var idempotency = new CheckoutIdempotency
        {
            Scope = scope,
            Key = idempotencyKey,
            RequestHash = requestHash,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(2)
        };
        var order = new Order
        {
            OrderNumber = CreateOrderNumber(),
            CustomerId = currentActor.CustomerId,
            DeliveryLocationId = state.Location.Id,
            DeliveryAreaNameArabic = state.Location.NameArabic,
            DeliveryAreaNameEnglish = state.Location.NameEnglish,
            Language = request.Language,
            CustomerName = state.CustomerName,
            PhoneNumber = state.PhoneNumber,
            City = state.Location.NameEnglish,
            Area = state.Area,
            AddressLine = state.AddressLine,
            TaxNumber = state.TaxNumber,
            DeliveryNotes = state.DeliveryNotes,
            Currency = state.Currency,
            Subtotal = state.Subtotal,
            DiscountAmount = state.Discount,
            DeliveryAmount = state.Delivery,
            TotalAmount = state.Total,
            Status = OrderStatus.New,
            SelectedPaymentMethod = request.PaymentMethod,
            CouponId = state.Cart.CouponId
        };
        foreach (CartItem cartItem in state.Cart.Items)
        {
            ProductVariant variant = cartItem.ProductVariant;
            InventoryItem inventory = variant.InventoryItem;
            int available = inventory.QuantityOnHand - inventory.ReservedQuantity;
            if (cartItem.Quantity > available)
            {
                throw new ConflictException($"Insufficient stock for SKU '{variant.Sku}'.");
            }

            inventory.ReservedQuantity += cartItem.Quantity;
            var inventoryTransaction = new InventoryTransaction
            {
                Type = InventoryTransactionType.Reservation,
                Quantity = cartItem.Quantity,
                Reference = order.OrderNumber,
                Notes = "Reserved during checkout."
            };
            inventory.Transactions.Add(inventoryTransaction);
            dbContext.InventoryTransactions.Add(inventoryTransaction);
            decimal unitPrice = variant.PriceOverride ?? variant.Product.BasePrice;
            order.Items.Add(new OrderItem { ProductId = variant.ProductId, ProductVariantId = variant.Id, ProductNameSnapshot = ResolveName(variant), SkuSnapshot = variant.Sku, VariantDescriptionSnapshot = string.Join(" / ", new[] { variant.Size, variant.Color }.Where(value => !string.IsNullOrWhiteSpace(value))), UnitPrice = unitPrice, Quantity = cartItem.Quantity, TotalPrice = unitPrice * cartItem.Quantity });
        }

        order.StatusHistory.Add(new OrderStatusHistory { NewStatus = OrderStatus.New, Note = "Order placed." });
        var payment = new Payment
        {
            Method = request.PaymentMethod,
            Status = PaymentStatus.Pending,
            Amount = order.TotalAmount,
            Currency = order.Currency,
            ProviderName = IsOnline(request.PaymentMethod) ? "Online payment gateway" : "Manual",
            TransactionReference = request.PaymentReference?.Trim() ?? string.Empty,
            ProviderResponseReference = string.Empty,
            ProofFilePath = string.Empty,
            VerificationNote = string.Empty
        };
        order.Payments.Add(payment);
        if (state.Cart.Coupon is not null)
        {
            order.CouponUsage = new CouponUsage
            {
                CouponId = state.Cart.Coupon.Id,
                CustomerId = currentActor.CustomerId,
                DiscountAmount = state.Discount
            };
        }

        string? paymentRedirectUrl = null;
        if (IsOnline(request.PaymentMethod))
        {
            OnlinePaymentResult onlinePayment = await onlinePaymentGateway.StartAsync(
                new OnlinePaymentRequest(
                    order.Id,
                    order.OrderNumber,
                    order.TotalAmount,
                    order.Currency,
                    idempotencyKey),
                cancellationToken);
            if (string.IsNullOrWhiteSpace(onlinePayment.ProviderReference)
                || onlinePayment.ProviderReference.Trim().Length > 250
                || string.IsNullOrWhiteSpace(onlinePayment.RedirectUrl)
                || onlinePayment.RedirectUrl.Trim().Length > 2048
                || !Uri.TryCreate(onlinePayment.RedirectUrl.Trim(), UriKind.Absolute, out Uri? redirectUri)
                || (redirectUri.Scheme != Uri.UriSchemeHttps
                    && redirectUri.Scheme != Uri.UriSchemeHttp))
            {
                throw new ServiceUnavailableException("The online payment provider returned an invalid checkout response.");
            }

            payment.Status = PaymentStatus.Processing;
            payment.ProviderResponseReference = onlinePayment.ProviderReference.Trim();
            paymentRedirectUrl = onlinePayment.RedirectUrl.Trim();
        }

        state.Cart.Status = CartStatus.Converted;
        state.Cart.GuestTokenHash = string.Empty;
        idempotency.Order = order;
        dbContext.CheckoutIdempotencies.Add(idempotency);
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        await notificationService.EnqueueAsync(new EnqueueNotificationRequest(null, null, order.Id, request.Language, NotificationChannel.InApp, "New order", $"Order {order.OrderNumber} was placed.", "administrators"), cancellationToken);
        if (order.CustomerId.HasValue)
        {
            await notificationService.EnqueueAsync(new EnqueueNotificationRequest(order.CustomerId, null, order.Id, request.Language, NotificationChannel.InApp, request.Language == LanguageCode.Arabic ? "تم استلام الطلب" : "Order received", request.Language == LanguageCode.Arabic ? $"تم استلام طلبك {order.OrderNumber}." : $"Your order {order.OrderNumber} was received.", order.PhoneNumber), cancellationToken);
        }

        return MapOrder(order, payment, false, paymentRedirectUrl);
    }
}
