using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public sealed record ProductListQuery(int Page = PaginationRules.DefaultPage, int PageSize = PaginationRules.DefaultPageSize, string? Search = null, Guid? CategoryId = null, decimal? MinPrice = null, decimal? MaxPrice = null, bool? IsFeatured = null, bool? InStock = null, string? Size = null, string? Color = null, bool? HasDiscount = null, string Sort = ProductSortValues.Newest);
