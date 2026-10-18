using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Banners.DTOs;
public sealed record BannerTranslationRequest(LanguageCode Language, string Title, string DesktopImageUrl, string MobileImageUrl, string AltText, string? LinkUrl);
