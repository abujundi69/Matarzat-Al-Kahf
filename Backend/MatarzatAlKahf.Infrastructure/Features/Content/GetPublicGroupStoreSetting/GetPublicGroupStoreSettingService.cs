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
    public async Task<IReadOnlyList<StoreSettingResponse>> GetPublicGroupAsync(string group, CancellationToken cancellationToken)
    {
        List<StoreSetting> settings = await Query(group).AsNoTracking().Where(setting => setting.IsPublic).OrderBy(setting => setting.Key).ToListAsync(cancellationToken);
        LanguageCode language = languageResolver.GetCurrentLanguage();
        return settings.Select(setting => MapPublic(setting, language)).ToArray();
    }
}
