using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Categories;
public sealed record PublicCategorySummaryDto(Guid Id, Guid? ParentCategoryId, string Name, string Slug);
