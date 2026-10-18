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
    public async Task<IReadOnlyList<DeliveryLocationResponse>> ListPublicAsync(CancellationToken cancellationToken)
    {
        bool english = languageResolver.GetCurrentLanguage() == Domain.Enums.LanguageCode.English;
        return await dbContext.DeliveryLocations.AsNoTracking().Where(location => location.IsActive && !location.IsDeleted && (location.CountryCode == "PS" || location.CountryCode == "ps" || location.CountryCode == "Ps" || location.CountryCode == "pS")).OrderBy(location => location.SortOrder).ThenBy(location => location.NameEnglish).Select(location => new DeliveryLocationResponse(location.Id, english ? location.NameEnglish : location.NameArabic, location.CashOnDeliveryAllowed, location.AdditionalDeliveryCost, location.MinimumOrderAmount)).ToListAsync(cancellationToken);
    }
}
