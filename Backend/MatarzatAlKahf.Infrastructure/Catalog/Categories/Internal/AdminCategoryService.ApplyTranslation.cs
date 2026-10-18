using MatarzatAlKahf.Application.Catalog.Categories;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MatarzatAlKahf.Infrastructure.Catalog.Categories;
public sealed partial class AdminCategoryService
{
    private static void ApplyTranslation(CategoryTranslation target, CategoryTranslationRequest input, string normalizedSlug)
    {
        target.Language = input.Language;
        target.Name = input.Name.Trim();
        target.Slug = normalizedSlug;
        target.Description = input.Description?.Trim() ?? string.Empty;
        target.MetaTitle = input.MetaTitle?.Trim() ?? string.Empty;
        target.MetaDescription = input.MetaDescription?.Trim() ?? string.Empty;
        target.IsDeleted = false;
        target.UpdatedAtUtc = DateTime.UtcNow;
    }
}
