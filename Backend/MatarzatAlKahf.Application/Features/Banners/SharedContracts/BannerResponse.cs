using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Banners.DTOs;
public sealed record BannerResponse(Guid Id, BannerPosition Position, int SortOrder, string Title, string DesktopImageUrl, string MobileImageUrl, string AltText, string? LinkUrl);
