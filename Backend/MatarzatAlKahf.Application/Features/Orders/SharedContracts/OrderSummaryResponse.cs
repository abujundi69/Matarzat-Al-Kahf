using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
public sealed record OrderSummaryResponse(Guid Id, string OrderNumber, string CustomerName, string PhoneNumber, OrderStatus Status, PaymentMethod PaymentMethod, PaymentStatus PaymentStatus, decimal Total, CurrencyCode Currency, DateTime CreatedAtUtc);
