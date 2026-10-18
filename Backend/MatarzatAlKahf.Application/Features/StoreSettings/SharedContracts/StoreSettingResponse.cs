using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.StoreSettings.DTOs;
public sealed record StoreSettingResponse(string Key, string? Value, string? Description, bool IsPublic, bool IsLocalized, IReadOnlyList<SettingTranslationRequest> Translations);
