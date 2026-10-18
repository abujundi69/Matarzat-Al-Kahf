using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Categories;
public interface IGetBySlugPublicCategoryService
{
    Task<PublicCategoryDto> GetBySlugAsync(string slug, CancellationToken cancellationToken);
}
