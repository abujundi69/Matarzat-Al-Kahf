using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public sealed record CreateProductRequest(Guid CategoryId, decimal BasePrice, CurrencyCode Currency, ProductStatus Status, bool IsFeatured, DateTime? PublishedAtUtc, IReadOnlyList<ProductTranslationRequest> Translations, IReadOnlyList<CreateProductVariantRequest> Variants, IReadOnlyList<CreateProductImageRequest>? Images = null);
