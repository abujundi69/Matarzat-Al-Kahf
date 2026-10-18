using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.ContentPages.DTOs;
public sealed record AdminContentPageResponse(Guid Id, ContentPageType Type, bool IsPublished, IReadOnlyList<ContentPageTranslationRequest> Translations, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);
