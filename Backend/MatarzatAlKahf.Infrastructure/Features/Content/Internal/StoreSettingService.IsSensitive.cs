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
    private static bool IsSensitive(string key) => key.Contains("Secret", StringComparison.OrdinalIgnoreCase) || key.Contains("Password", StringComparison.OrdinalIgnoreCase) || key.Contains("Token", StringComparison.OrdinalIgnoreCase) || key.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase);
}
