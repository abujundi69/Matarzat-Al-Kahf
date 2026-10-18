using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Categories;
public interface IGetAllPublicCategoryService
{
    Task<IReadOnlyList<PublicCategoryDto>> GetAllAsync(Guid? parentCategoryId, CancellationToken cancellationToken);
}
