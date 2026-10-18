using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Validation;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public interface IProductRequestValidator
{
    void Validate(CreateProductRequest request);
    void Validate(UpdateProductRequest request);
    void Validate(ProductListQuery query);
}
