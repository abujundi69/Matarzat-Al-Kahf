using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public sealed record ProductTranslationRequest(LanguageCode Language, string Name, string Slug, string? ShortDescription, string Description, string? Story, string? MetaTitle, string? MetaDescription);
