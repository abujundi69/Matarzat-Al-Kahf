using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Validation;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Categories;
public static class CategoryHierarchyRules
{
    public static void EnsureNoCycle(Guid categoryId, Guid? parentCategoryId, IReadOnlyCollection<Guid> prospectiveParentAncestorIds)
    {
        ArgumentNullException.ThrowIfNull(prospectiveParentAncestorIds);
        if (!parentCategoryId.HasValue)
        {
            return;
        }

        if (parentCategoryId.Value == categoryId)
        {
            throw new ConflictException("A category cannot be its own parent.");
        }

        if (prospectiveParentAncestorIds.Contains(categoryId))
        {
            throw new ConflictException("The selected parent would create a category hierarchy cycle.");
        }
    }
}
