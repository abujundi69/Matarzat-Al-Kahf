namespace MatarzatAlKahf.Application.Features.Seo.DTOs;
public sealed record SeoSettingsResponse(string BaseUrl, string ArabicTitle, string EnglishTitle, string ArabicDescription, string EnglishDescription, string? ArabicOpenGraphImageUrl, string? EnglishOpenGraphImageUrl, string? RobotsAdditionalRules);
