using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Categories;
public sealed record AdminCategoryDto(Guid Id, Guid? ParentCategoryId, string? ImageUrl, int SortOrder, bool IsActive, IReadOnlyList<CategoryTranslationDto> Translations);
