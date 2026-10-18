namespace MatarzatAlKahf.Application.Features.Delivery.DTOs.Responses;
public sealed record DeliveryLocationResponse(Guid Id, string Name, bool CashOnDeliveryAllowed, decimal AdditionalDeliveryCost, decimal? MinimumOrderAmount);
