using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public sealed record AdminProductImageDto(Guid Id, string ImageUrl, string ThumbnailUrl, int SortOrder, bool IsPrimary, IReadOnlyList<ProductImageTranslationDto> Translations);
