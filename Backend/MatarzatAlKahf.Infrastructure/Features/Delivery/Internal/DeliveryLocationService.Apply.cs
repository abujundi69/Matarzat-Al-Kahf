using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Features.Delivery.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Delivery.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Delivery.Interfaces;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Delivery.Services;
public sealed partial class DeliveryLocationService
{
    private static void Apply(DeliveryLocation location, SaveDeliveryLocationRequest request)
    {
        location.CountryCode = "PS";
        location.CountryNameArabic = "فلسطين";
        location.CountryNameEnglish = "Palestine";
        location.NameArabic = request.NameArabic.Trim();
        location.NameEnglish = request.NameEnglish.Trim();
        location.IsActive = request.IsActive;
        location.CashOnDeliveryAllowed = request.CashOnDeliveryAllowed;
        location.ProductPriceIncludesDelivery = false;
        location.AdditionalDeliveryCost = request.AdditionalDeliveryCost!.Value;
        location.MinimumOrderAmount = request.MinimumOrderAmount;
        location.SortOrder = request.SortOrder;
    }
}
