using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Checkout.DTOs.Responses;
public sealed record CheckoutItemResponse(Guid ProductVariantId, string Name, string Sku, decimal UnitPrice, int Quantity, decimal Total);
