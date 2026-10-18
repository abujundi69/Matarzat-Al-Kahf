using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Categories;
public interface ICreateAdminCategoryService
{
    Task<AdminCategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken);
}
