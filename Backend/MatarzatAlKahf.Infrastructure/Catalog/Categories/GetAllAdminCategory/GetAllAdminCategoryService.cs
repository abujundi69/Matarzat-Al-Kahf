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
    public async Task<PagedResult<AdminCategoryDto>> GetAllAsync(Guid? parentCategoryId, int page, int pageSize, CancellationToken cancellationToken)
    {
        PaginationRules.Validate(page, pageSize);
        IQueryable<Category> query = _dbContext.Categories.AsNoTracking().Where(category => !category.IsDeleted);
        if (parentCategoryId.HasValue)
        {
            query = query.Where(category => category.ParentCategoryId == parentCategoryId);
        }

        int totalItems = await query.CountAsync(cancellationToken);
        List<Category> categories = await query.Include(category => category.Translations).AsSplitQuery().OrderBy(category => category.SortOrder).ThenBy(category => category.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return PagedResultFactory.Create(categories.Select(MapAdmin).ToList(), page, pageSize, totalItems);
    }
}
