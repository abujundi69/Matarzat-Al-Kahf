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
    public async Task<AdminCategoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        Category? category = await _dbContext.Categories.AsNoTracking().Include(entity => entity.Translations).Where(entity => entity.Id == id && !entity.IsDeleted).SingleOrDefaultAsync(cancellationToken);
        return category is null ? throw new NotFoundException(nameof(Category), id) : MapAdmin(category);
    }
}
