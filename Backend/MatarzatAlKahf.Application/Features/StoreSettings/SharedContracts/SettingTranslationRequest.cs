using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.StoreSettings.DTOs;
public sealed record SettingTranslationRequest(LanguageCode Language, string Value);
