using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Categories;
public sealed record CategoryTranslationDto(LanguageCode Language, string Name, string Slug, string? Description, string? MetaTitle, string? MetaDescription);
