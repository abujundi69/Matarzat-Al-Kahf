using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
public sealed record OrderStatusResponse(OrderStatus? PreviousStatus, OrderStatus Status, string? Note, DateTime ChangedAtUtc);
