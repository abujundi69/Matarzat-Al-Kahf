namespace MatarzatAlKahf.Application.Features.Delivery.DTOs.Responses;
public sealed record AdminDeliveryLocationResponse(Guid Id, string NameArabic, string NameEnglish, bool IsActive, bool CashOnDeliveryAllowed, decimal AdditionalDeliveryCost, decimal? MinimumOrderAmount, int SortOrder);
