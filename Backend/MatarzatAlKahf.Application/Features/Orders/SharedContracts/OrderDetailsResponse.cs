using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
public sealed record OrderDetailsResponse(Guid Id, string OrderNumber, Guid? CustomerId, string CustomerName, string PhoneNumber, string DeliveryAreaName, string City, string Area, string AddressLine, string? TaxNumber, string? DeliveryNotes, string? InternalNotes, OrderStatus Status, PaymentMethod PaymentMethod, decimal Subtotal, decimal Discount, decimal Delivery, decimal Total, CurrencyCode Currency, DateTime CreatedAtUtc, IReadOnlyList<OrderItemResponse> Items, IReadOnlyList<OrderStatusResponse> StatusHistory, IReadOnlyList<OrderPaymentResponse> Payments);
