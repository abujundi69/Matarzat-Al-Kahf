using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.ContentPages.DTOs;
public sealed record SaveContentPageRequest(ContentPageType Type, bool IsPublished, IReadOnlyList<ContentPageTranslationRequest> Translations);
