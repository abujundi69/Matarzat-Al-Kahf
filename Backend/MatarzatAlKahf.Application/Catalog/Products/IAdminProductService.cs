using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public interface IAdminProductService : IGetAllAdminProductService, IGetByIdAdminProductService, ICreateAdminProductService, IUpdateAdminProductService, IDeleteAdminProductService
{
}
