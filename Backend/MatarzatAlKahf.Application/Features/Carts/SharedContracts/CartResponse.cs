using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Carts.DTOs.Responses;
public sealed record CartResponse(Guid Id, string? CartToken, IReadOnlyList<CartItemResponse> Items, string? CouponCode, decimal Subtotal, decimal Discount, decimal Total, CurrencyCode Currency, DateTime ExpiresAtUtc, string ConcurrencyToken);
