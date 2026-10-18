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
    private async Task<IReadOnlyCollection<Guid>> GetAncestorIdsAsync(Guid? parentCategoryId, CancellationToken cancellationToken)
    {
        var ancestorIds = new HashSet<Guid>();
        Guid? currentId = parentCategoryId;
        while (currentId.HasValue && ancestorIds.Add(currentId.Value))
        {
            currentId = await _dbContext.Categories.AsNoTracking().Where(category => category.Id == currentId.Value && !category.IsDeleted).Select(category => category.ParentCategoryId).SingleOrDefaultAsync(cancellationToken);
        }

        return ancestorIds;
    }
}
