using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Categories;
public sealed record PublicCategoryDto(Guid Id, Guid? ParentCategoryId, string? ImageUrl, int SortOrder, string Name, string Slug, string? Description, string? MetaTitle, string? MetaDescription, int ProductCount, IReadOnlyList<PublicCategorySummaryDto> Children);
