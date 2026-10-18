using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Categories;
public interface IDeleteAdminCategoryService
{
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
