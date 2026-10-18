using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.SocialMediaLinks.DTOs;
public sealed record SaveSocialLinkRequest(SocialPlatform Platform, string Url, bool IsActive, int SortOrder);
