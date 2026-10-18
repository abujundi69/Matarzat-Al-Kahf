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
    private IQueryable<StoreSetting> Query(string group)
    {
        if (string.IsNullOrWhiteSpace(group) || group.Trim().Length > 100)
        {
            throw ValidationException.For(nameof(group), "Setting group is invalid.");
        }

        string normalized = group.Trim();
        return dbContext.StoreSettings.Include(setting => setting.Translations).Where(setting => setting.Group == normalized && !setting.IsDeleted && !setting.Key.Contains("Secret") && !setting.Key.Contains("Password") && !setting.Key.Contains("Token") && !setting.Key.Contains("ConnectionString"));
    }
}
