using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public interface IPublicProductService : IGetPagePublicProductService, IGetByIdPublicProductService, IGetBySlugPublicProductService, IGetSimilarPublicProductService
{
}
