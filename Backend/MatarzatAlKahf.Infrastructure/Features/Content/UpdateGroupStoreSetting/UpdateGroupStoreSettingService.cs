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
    public async Task<IReadOnlyList<StoreSettingResponse>> UpdateGroupAsync(string group, SaveSettingGroupRequest request, CancellationToken cancellationToken)
    {
        ValidateGroup(group, request);
        List<StoreSetting> existing = await Query(group).ToListAsync(cancellationToken);
        string[] previousValues = existing
            .SelectMany(setting => setting.Translations
                .Select(translation => translation.Value)
                .Append(setting.Value))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        Dictionary<string, StoreSetting> byKey = existing.ToDictionary(setting => setting.Key, StringComparer.OrdinalIgnoreCase);
        foreach (SaveSettingRequest item in request.Settings)
        {
            string settingKey = NormalizeSettingKey(item.Key);
            bool isCurrencySetting = IsCurrencySetting(settingKey);
            bool isLocalized = isCurrencySetting ? false : item.IsLocalized;
            if (!byKey.TryGetValue(settingKey, out StoreSetting? setting))
            {
                setting = new StoreSetting
                {
                    Key = settingKey,
                    Group = group.Trim()
                };
                dbContext.StoreSettings.Add(setting);
                byKey[setting.Key] = setting;
            }

            setting.Value = isLocalized
                ? null
                : isCurrencySetting ? "ILS" : item.Value?.Trim();
            setting.Description = item.Description?.Trim() ?? string.Empty;
            setting.IsPublic = item.IsPublic;
            setting.IsLocalized = isLocalized;
            setting.IsDeleted = false;
            setting.UpdatedAtUtc = DateTime.UtcNow;

            if (isLocalized)
            {
                Dictionary<LanguageCode, StoreSettingTranslation> translationsByLanguage =
                    setting.Translations.ToDictionary(
                        translation => translation.Language);

                foreach (SettingTranslationRequest translation in item.Translations)
                {
                    if (translationsByLanguage.TryGetValue(
                            translation.Language,
                            out StoreSettingTranslation? existingTranslation))
                    {
                        existingTranslation.Value = translation.Value.Trim();
                        existingTranslation.IsDeleted = false;
                        existingTranslation.UpdatedAtUtc = DateTime.UtcNow;
                        continue;
                    }

                    var newTranslation = new StoreSettingTranslation
                    {
                        StoreSettingId = setting.Id,
                        Language = translation.Language,
                        Value = translation.Value.Trim()
                    };
                    setting.Translations.Add(newTranslation);
                    dbContext.StoreSettingTranslations.Add(newTranslation);
                }
            }
            else if (setting.Translations.Count > 0)
            {
                dbContext.StoreSettingTranslations.RemoveRange(setting.Translations);
                setting.Translations.Clear();
            }
        }

        string[] incoming = request.Settings.Select(item => NormalizeSettingKey(item.Key)).ToArray();
        foreach (StoreSetting setting in existing.Where(item => !incoming.Contains(item.Key, StringComparer.OrdinalIgnoreCase)))
        {
            setting.IsDeleted = true;
            setting.UpdatedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        foreach (string previousValue in previousValues)
        {
            await fileCleanup.DeleteIfUnreferencedAsync(
                previousValue,
                cancellationToken);
        }

        await auditService.RecordAsync("settings.group.updated", nameof(StoreSetting), null, $"Settings group '{group}' was updated.", cancellationToken);
        return await GetAdminGroupAsync(group, cancellationToken);
    }

    private static bool IsCurrencySetting(string key) =>
        key.Equals("store.currency", StringComparison.OrdinalIgnoreCase)
        || key.Equals(
            "store.supportedCurrencies",
            StringComparison.OrdinalIgnoreCase);

    private static string NormalizeSettingKey(string key)
    {
        string trimmed = key.Trim();
        if (trimmed.Equals("store.currency", StringComparison.OrdinalIgnoreCase))
        {
            return "store.currency";
        }

        return trimmed.Equals(
            "store.supportedCurrencies",
            StringComparison.OrdinalIgnoreCase)
            ? "store.supportedCurrencies"
            : trimmed;
    }
}
