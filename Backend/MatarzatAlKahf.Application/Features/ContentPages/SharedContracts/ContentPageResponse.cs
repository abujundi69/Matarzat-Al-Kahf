using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.ContentPages.DTOs;
public sealed record ContentPageResponse(Guid Id, ContentPageType Type, string Title, string Slug, string Content, string? MetaTitle, string? MetaDescription, DateTime? UpdatedAtUtc);
