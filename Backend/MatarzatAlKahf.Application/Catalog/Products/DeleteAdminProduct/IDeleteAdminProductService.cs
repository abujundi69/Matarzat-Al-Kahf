using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public interface IDeleteAdminProductService
{
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
