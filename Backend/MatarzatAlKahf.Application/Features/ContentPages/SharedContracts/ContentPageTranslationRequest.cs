using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.ContentPages.DTOs;
public sealed record ContentPageTranslationRequest(LanguageCode Language, string Title, string Slug, string Content, string? MetaTitle, string? MetaDescription);
