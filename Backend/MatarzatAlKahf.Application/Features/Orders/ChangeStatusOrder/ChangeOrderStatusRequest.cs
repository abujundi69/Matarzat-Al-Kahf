using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Orders.DTOs.Requests;

public sealed record ChangeOrderStatusRequest(OrderStatus Status, string? Note);
