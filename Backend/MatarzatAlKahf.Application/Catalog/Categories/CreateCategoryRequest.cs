using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Categories;
public sealed record CreateCategoryRequest(Guid? ParentCategoryId, string? ImageUrl, int SortOrder, bool IsActive, IReadOnlyList<CategoryTranslationRequest> Translations);
