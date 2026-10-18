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
    private IQueryable<Product> ProductGraph(bool asNoTracking)
    {
        IQueryable<Product> query = _dbContext.Products.Include(product => product.Translations).Include(product => product.Images).ThenInclude(image => image.Translations).Include(product => product.Variants).ThenInclude(variant => variant.InventoryItem).AsSplitQuery();
        return asNoTracking ? query.AsNoTracking() : query;
    }
}
