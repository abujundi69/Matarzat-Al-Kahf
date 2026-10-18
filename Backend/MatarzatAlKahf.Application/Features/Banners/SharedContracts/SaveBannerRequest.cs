using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Banners.DTOs;
public sealed record SaveBannerRequest(BannerPosition Position, int SortOrder, bool IsActive, DateTime? StartsAtUtc, DateTime? EndsAtUtc, IReadOnlyList<BannerTranslationRequest> Translations);
