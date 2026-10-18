using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public sealed record PublicProductListItemDto(Guid Id, string Name, string Slug, PublicProductCategoryDto Category, decimal BasePrice, decimal MinimumEffectivePrice, CurrencyCode Currency, bool IsFeatured, string? PrimaryImageUrl, string? PrimaryImageAltText, bool InStock, DateTime? PublishedAtUtc);
