using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public sealed record AdminProductDto(Guid Id, Guid CategoryId, decimal BasePrice, CurrencyCode Currency, ProductStatus Status, bool IsFeatured, DateTime? PublishedAtUtc, IReadOnlyList<ProductTranslationDto> Translations, IReadOnlyList<AdminProductImageDto> Images, IReadOnlyList<AdminProductVariantDto> Variants);
