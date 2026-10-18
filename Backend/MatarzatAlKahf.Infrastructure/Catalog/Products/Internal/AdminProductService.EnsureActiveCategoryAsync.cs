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
    private async Task EnsureActiveCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        bool exists = await _dbContext.Categories.AnyAsync(category => category.Id == categoryId && !category.IsDeleted && category.IsActive, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException("Active category", categoryId);
        }
    }
}
