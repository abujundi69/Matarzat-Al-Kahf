using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public sealed record CreateProductImageRequest(string ImageUrl, string ThumbnailUrl, int SortOrder, bool IsPrimary, IReadOnlyList<ProductImageTranslationRequest> Translations);
