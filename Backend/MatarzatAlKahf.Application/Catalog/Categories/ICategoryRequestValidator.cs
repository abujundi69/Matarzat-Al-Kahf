using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Validation;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Categories;
public interface ICategoryRequestValidator
{
    void Validate(CreateCategoryRequest request);
    void Validate(UpdateCategoryRequest request);
    void EnsureValidParent(Guid categoryId, Guid? parentCategoryId, IReadOnlyCollection<Guid> prospectiveParentAncestorIds);
}
