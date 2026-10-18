using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public sealed record UpdateProductRequest(Guid CategoryId, decimal BasePrice, CurrencyCode Currency, ProductStatus Status, bool IsFeatured, DateTime? PublishedAtUtc, IReadOnlyList<ProductTranslationRequest> Translations, IReadOnlyList<UpdateProductVariantRequest> Variants, IReadOnlyList<UpdateProductImageRequest>? Images = null);
