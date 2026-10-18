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
    private static void Validate(SaveDeliveryLocationRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        Check(request.NameArabic, nameof(request.NameArabic), errors);
        Check(request.NameEnglish, nameof(request.NameEnglish), errors);
        if (!request.AdditionalDeliveryCost.HasValue)
        {
            errors[nameof(request.AdditionalDeliveryCost)] = ["Delivery price is required."];
        }
        else if (request.AdditionalDeliveryCost < 0)
        {
            errors[nameof(request.AdditionalDeliveryCost)] = ["Delivery price cannot be negative."];
        }

        if (request.MinimumOrderAmount < 0)
        {
            errors[nameof(request.MinimumOrderAmount)] = ["Minimum order amount cannot be negative."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Delivery location is invalid.", errors);
        }
    }
}
