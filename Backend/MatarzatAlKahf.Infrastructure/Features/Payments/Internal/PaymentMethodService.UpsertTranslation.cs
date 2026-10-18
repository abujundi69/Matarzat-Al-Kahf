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
    private static void UpsertTranslation(StoreSetting setting, LanguageCode language, string value)
    {
        StoreSettingTranslation? translation = setting.Translations.FirstOrDefault(item => item.Language == language);
        if (translation is null)
        {
            setting.Translations.Add(new StoreSettingTranslation { Language = language, Value = value });
        }
        else
        {
            translation.Value = value;
            translation.IsDeleted = false;
            translation.UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
