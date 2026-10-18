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
    public async Task<AdminProductDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        Product? product = await ProductGraph(asNoTracking: true).SingleOrDefaultAsync(entity => entity.Id == id && !entity.IsDeleted, cancellationToken);
        return product is null ? throw new NotFoundException(nameof(Product), id) : ProductDtoMapper.MapAdmin(product);
    }
}
