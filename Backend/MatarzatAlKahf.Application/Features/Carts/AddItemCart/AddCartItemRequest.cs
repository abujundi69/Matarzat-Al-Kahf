namespace MatarzatAlKahf.Application.Features.Carts.DTOs.Requests;

public sealed record AddCartItemRequest(Guid ProductVariantId, int Quantity);
