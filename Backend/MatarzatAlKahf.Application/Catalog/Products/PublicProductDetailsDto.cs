using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public sealed record PublicProductDetailsDto(Guid Id, string Name, string Slug, string? ShortDescription, string Description, string? Story, string? MetaTitle, string? MetaDescription, PublicProductCategoryDto Category, decimal BasePrice, CurrencyCode Currency, bool IsFeatured, IReadOnlyList<PublicProductImageDto> Images, IReadOnlyList<PublicProductVariantDto> Variants, DateTime? PublishedAtUtc);
