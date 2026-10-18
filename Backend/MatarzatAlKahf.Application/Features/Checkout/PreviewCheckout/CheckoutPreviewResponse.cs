using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Checkout.DTOs.Responses;
public sealed record CheckoutPreviewResponse(Guid CartId, decimal Subtotal, decimal Discount, decimal Delivery, decimal Total, CurrencyCode Currency, PaymentMethod PaymentMethod, string PaymentInstructions, IReadOnlyList<CheckoutItemResponse> Items);
