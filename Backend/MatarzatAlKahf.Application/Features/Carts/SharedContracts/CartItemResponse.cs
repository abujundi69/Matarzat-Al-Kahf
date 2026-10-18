using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Carts.DTOs.Responses;
public sealed record CartItemResponse(Guid Id, Guid ProductId, Guid ProductVariantId, string Name, string Sku, string? Size, string? Color, decimal UnitPrice, int Quantity, decimal Total, int AvailableQuantity);
