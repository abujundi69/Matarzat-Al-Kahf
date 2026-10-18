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
    private static StoreSettingResponse MapAdmin(StoreSetting setting)
    {
        bool isCurrencySetting = IsCurrencySetting(setting.Key);
        return new StoreSettingResponse(
            setting.Key,
            isCurrencySetting ? "ILS" : setting.Value,
            setting.Description,
            setting.IsPublic,
            isCurrencySetting ? false : setting.IsLocalized,
            isCurrencySetting
                ? []
                : setting.Translations
                    .OrderBy(item => item.Language)
                    .Select(item => new SettingTranslationRequest(
                        item.Language,
                        item.Value))
                    .ToArray());
    }
}
