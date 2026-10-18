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
    private const int MaxSettingKeyLength = 200;
    private const int MaxSettingGroupLength = 100;
    private const int MaxSettingTextLength = 4000;

    private static void ValidateGroup(string group, SaveSettingGroupRequest request)
    {
        if (string.IsNullOrWhiteSpace(group) || group.Trim().Length > MaxSettingGroupLength)
        {
            throw ValidationException.For(nameof(group), "Setting group is invalid.");
        }

        if (request is null || request.Settings is null)
        {
            throw ValidationException.For(nameof(request), "Settings are required.");
        }

        if (request.Settings.Any(item => item is null))
        {
            throw ValidationException.For(nameof(request.Settings), "Settings cannot contain null entries.");
        }

        if (request.Settings.Any(item =>
                string.IsNullOrWhiteSpace(item.Key) ||
                item.Key.Trim().Length > MaxSettingKeyLength ||
                IsSensitive(item.Key)) ||
            request.Settings
                .Select(item => item.Key.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() != request.Settings.Count)
        {
            throw ValidationException.For(nameof(request.Settings), "Setting keys are invalid.");
        }

        if (request.Settings.Any(item =>
                item.Value?.Trim().Length > MaxSettingTextLength ||
                item.Description?.Trim().Length > MaxSettingTextLength))
        {
            throw ValidationException.For(nameof(request.Settings), "Setting values and descriptions are limited to 4000 characters.");
        }

        if (request.Settings.Any(item => item.Translations is null))
        {
            throw ValidationException.For(nameof(request.Settings), "Setting translations are required.");
        }

        if (request.Settings.Any(item => item.Translations.Any(translation =>
                translation is null ||
                translation.Value is null ||
                translation.Value.Trim().Length > MaxSettingTextLength)))
        {
            throw ValidationException.For(nameof(request.Settings), "Setting translations are limited to 4000 characters.");
        }

        if (request.Settings.Any(item => item.IsLocalized &&
                (item.Translations.Count != 2 ||
                 item.Translations.Select(value => value.Language).Distinct().Count() != 2 ||
                 !item.Translations.Any(value => value.Language == LanguageCode.Arabic) ||
                 !item.Translations.Any(value => value.Language == LanguageCode.English) ||
                 item.Translations.Any(value => string.IsNullOrWhiteSpace(value.Value)))))
        {
            throw ValidationException.For(nameof(request.Settings), "Localized settings require Arabic and English values.");
        }
    }
}
