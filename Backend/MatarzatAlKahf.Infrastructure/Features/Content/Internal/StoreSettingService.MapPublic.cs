using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Features.StoreSettings.DTOs;
using MatarzatAlKahf.Application.Features.StoreSettings.Interfaces;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Content.Services;
public sealed partial class StoreSettingService
{
    private static StoreSettingResponse MapPublic(StoreSetting setting, LanguageCode language)
    {
        bool isCurrencySetting = IsCurrencySetting(setting.Key);
        string? value = isCurrencySetting
            ? "ILS"
            : setting.IsLocalized
                ? setting.Translations.FirstOrDefault(item => item.Language == language)?.Value
                    ?? setting.Translations.FirstOrDefault(item => item.Language == LanguageCode.Arabic)?.Value
                : setting.Value;
        return new StoreSettingResponse(
            setting.Key,
            value,
            null,
            true,
            isCurrencySetting ? false : setting.IsLocalized,
            []);
    }
}
