using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Categories;
public interface IGetAllAdminCategoryService
{
    Task<PagedResult<AdminCategoryDto>> GetAllAsync(Guid? parentCategoryId, int page, int pageSize, CancellationToken cancellationToken);
}
