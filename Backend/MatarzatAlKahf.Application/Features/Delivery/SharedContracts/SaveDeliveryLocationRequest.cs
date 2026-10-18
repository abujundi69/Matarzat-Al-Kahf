namespace MatarzatAlKahf.Application.Features.Delivery.DTOs.Requests;

public sealed record SaveDeliveryLocationRequest(
    string NameArabic,
    string NameEnglish,
    decimal? AdditionalDeliveryCost,
    bool IsActive = true,
    bool CashOnDeliveryAllowed = true,
    decimal? MinimumOrderAmount = null,
    int SortOrder = 0);
