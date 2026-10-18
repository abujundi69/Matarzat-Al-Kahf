using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Categories;
public interface IGetByIdAdminCategoryService
{
    Task<AdminCategoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
