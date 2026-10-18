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
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        Category category = await _dbContext.Categories.SingleOrDefaultAsync(entity => entity.Id == id && !entity.IsDeleted, cancellationToken) ?? throw new NotFoundException(nameof(Category), id);
        bool hasChildren = await _dbContext.Categories.AnyAsync(child => child.ParentCategoryId == id && !child.IsDeleted, cancellationToken);
        if (hasChildren)
        {
            throw new ConflictException("A category with child categories cannot be deleted.");
        }

        bool hasProducts = await _dbContext.Products.AnyAsync(product => product.CategoryId == id && !product.IsDeleted, cancellationToken);
        if (hasProducts)
        {
            throw new ConflictException("A category containing products cannot be deleted.");
        }

        category.IsDeleted = true;
        category.IsActive = false;
        category.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _fileCleanup.DeleteIfUnreferencedAsync(
            category.ImageUrl,
            cancellationToken);
        LogCategoryDeleted(_logger, id, null);
    }
}
