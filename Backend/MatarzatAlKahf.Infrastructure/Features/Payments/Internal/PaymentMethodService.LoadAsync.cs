using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Payments.DTOs;
using MatarzatAlKahf.Application.Features.Payments.Interfaces;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Payments.Services;
public sealed partial class PaymentMethodService
{
    private async Task<IReadOnlyList<PaymentMethodResponse>> LoadAsync(CancellationToken cancellationToken)
    {
        List<StoreSetting> settings = await dbContext.StoreSettings.AsNoTracking().Include(item => item.Translations.Where(translation => !translation.IsDeleted)).Where(item => item.Group == Group && !item.IsDeleted).ToListAsync(cancellationToken);
        LanguageCode language = languageResolver.GetCurrentLanguage();
        return Enum.GetValues<PaymentMethod>().Select(method =>
        {
            StoreSetting? setting = settings.SingleOrDefault(item => item.Key == Key(method));
            return setting is null ? new PaymentMethodResponse(method, DefaultEnabled(method), DefaultInstructions(method, language)) : Map(setting, method, language);
        }).ToArray();
    }
}
