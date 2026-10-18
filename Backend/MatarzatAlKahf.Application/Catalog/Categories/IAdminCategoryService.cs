using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Categories;
public interface IAdminCategoryService : IGetAllAdminCategoryService, IGetByIdAdminCategoryService, ICreateAdminCategoryService, IUpdateAdminCategoryService, IDeleteAdminCategoryService
{
}
