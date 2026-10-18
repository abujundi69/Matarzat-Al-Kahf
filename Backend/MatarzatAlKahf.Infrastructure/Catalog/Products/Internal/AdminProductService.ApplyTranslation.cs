using MatarzatAlKahf.Application.Catalog.Products;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MatarzatAlKahf.Infrastructure.Catalog.Products;
public sealed partial class AdminProductService
{
    private static void ApplyTranslation(ProductTranslation target, ProductTranslationRequest input, string normalizedSlug)
    {
        target.Language = input.Language;
        target.Name = input.Name.Trim();
        target.Slug = normalizedSlug;
        target.ShortDescription = input.ShortDescription?.Trim() ?? string.Empty;
        target.Description = input.Description.Trim();
        target.Story = input.Story?.Trim() ?? string.Empty;
        target.MetaTitle = input.MetaTitle?.Trim() ?? string.Empty;
        target.MetaDescription = input.MetaDescription?.Trim() ?? string.Empty;
        target.IsDeleted = false;
        target.UpdatedAtUtc = DateTime.UtcNow;
    }
}
