using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Categories;
public sealed record CategoryTranslationRequest(LanguageCode Language, string Name, string Slug, string? Description, string? MetaTitle, string? MetaDescription);
