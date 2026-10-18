using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
public sealed record OrderItemResponse(Guid ProductId, Guid? ProductVariantId, string Name, string Sku, string Variant, decimal UnitPrice, int Quantity, decimal Total);
