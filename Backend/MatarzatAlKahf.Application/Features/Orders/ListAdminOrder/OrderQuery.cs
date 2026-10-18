using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Orders.DTOs.Requests;

public sealed record OrderQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    OrderStatus? Status = null,
    PaymentMethod? PaymentMethod = null,
    PaymentStatus? PaymentStatus = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null);
