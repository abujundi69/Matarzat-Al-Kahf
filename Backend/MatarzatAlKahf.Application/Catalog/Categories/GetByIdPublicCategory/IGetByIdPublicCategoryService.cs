using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Categories;
public interface IGetByIdPublicCategoryService
{
    Task<PublicCategoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
