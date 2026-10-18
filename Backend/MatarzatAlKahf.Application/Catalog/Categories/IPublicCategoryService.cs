using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Categories;
public interface IPublicCategoryService : IGetAllPublicCategoryService, IGetByIdPublicCategoryService, IGetBySlugPublicCategoryService
{
}
