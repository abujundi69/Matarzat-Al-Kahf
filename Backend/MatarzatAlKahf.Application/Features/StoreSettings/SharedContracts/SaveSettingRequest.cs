using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.StoreSettings.DTOs;
public sealed record SaveSettingRequest(string Key, string? Value, string? Description, bool IsPublic, bool IsLocalized, IReadOnlyList<SettingTranslationRequest> Translations);
