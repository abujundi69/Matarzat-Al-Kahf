using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Checkout.DTOs.Responses;

public sealed record PlaceOrderResponse(
    Guid OrderId,
    string OrderNumber,
    OrderStatus Status,
    Guid PaymentId,
    PaymentMethod PaymentMethod,
    PaymentStatus PaymentStatus,
    decimal Total,
    CurrencyCode Currency,
    string PaymentInstructions,
    string? PaymentRedirectUrl,
    bool IsDuplicate);
