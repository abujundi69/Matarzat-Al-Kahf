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
    public async Task<PagedResult<AdminProductDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        PaginationRules.Validate(page, pageSize);
        IQueryable<Product> query = ProductGraph(asNoTracking: true).Where(product => !product.IsDeleted).AsSplitQuery();
        int totalItems = await query.CountAsync(cancellationToken);
        List<Product> products = await query.OrderByDescending(product => product.CreatedAtUtc).ThenBy(product => product.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return PagedResultFactory.Create(products.Select(ProductDtoMapper.MapAdmin).ToList(), page, pageSize, totalItems);
    }
}
