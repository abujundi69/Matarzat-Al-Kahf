using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Validation;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Categories;
public sealed class CategoryRequestValidator(ISlugNormalizer slugNormalizer) : ICategoryRequestValidator
{
    private readonly ISlugNormalizer _slugNormalizer = slugNormalizer ?? throw new ArgumentNullException(nameof(slugNormalizer));
    public void Validate(CreateCategoryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCore(request.ImageUrl, request.SortOrder, request.Translations);
    }

    public void Validate(UpdateCategoryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCore(request.ImageUrl, request.SortOrder, request.Translations);
    }

    public void EnsureValidParent(Guid categoryId, Guid? parentCategoryId, IReadOnlyCollection<Guid> prospectiveParentAncestorIds)
    {
        CategoryHierarchyRules.EnsureNoCycle(categoryId, parentCategoryId, prospectiveParentAncestorIds);
    }

    private void ValidateCore(string? imageUrl, int sortOrder, IReadOnlyList<CategoryTranslationRequest> translations)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        ValidationRuleUtilities.AddIf(errors, sortOrder < 0, nameof(sortOrder), "Sort order cannot be negative.");
        ValidationRuleUtilities.AddIf(errors, imageUrl?.Length > 2000, nameof(imageUrl), "Image URL cannot exceed 2000 characters.");
        ValidateTranslations(translations, errors);
        ValidationRuleUtilities.ThrowIfAny(errors, "Category is invalid.");
    }

    private void ValidateTranslations(IReadOnlyList<CategoryTranslationRequest> translations, Dictionary<string, List<string>> errors)
    {
        if (translations is null)
        {
            ValidationRuleUtilities.Add(errors, nameof(translations), "Arabic and English translations are required.");
            return;
        }

        ValidationRuleUtilities.ValidateRequiredLanguages(translations.Select(translation => translation.Language), nameof(translations), errors);
        for (int index = 0; index < translations.Count; index++)
        {
            CategoryTranslationRequest translation = translations[index];
            string prefix = $"{nameof(translations)}[{index}]";
            ValidationRuleUtilities.AddIf(errors, string.IsNullOrWhiteSpace(translation.Name), $"{prefix}.{nameof(translation.Name)}", "Category name is required.");
            ValidationRuleUtilities.AddIf(errors, translation.Name?.Length > 200, $"{prefix}.{nameof(translation.Name)}", "Category name cannot exceed 200 characters.");
            if (!string.IsNullOrWhiteSpace(translation.Slug))
            {
                string normalizedSlug = _slugNormalizer.Normalize(translation.Slug);
                ValidationRuleUtilities.AddIf(errors, normalizedSlug.Length > 250, $"{prefix}.{nameof(translation.Slug)}", "Category slug cannot exceed 250 characters.");
            }
            else
            {
                ValidationRuleUtilities.Add(errors, $"{prefix}.{nameof(translation.Slug)}", "Category slug is required.");
            }

            ValidationRuleUtilities.AddIf(errors, translation.Description?.Length > 4000, $"{prefix}.{nameof(translation.Description)}", "Description cannot exceed 4000 characters.");
            ValidationRuleUtilities.AddIf(errors, translation.MetaTitle?.Length > 250, $"{prefix}.{nameof(translation.MetaTitle)}", "Meta title cannot exceed 250 characters.");
            ValidationRuleUtilities.AddIf(errors, translation.MetaDescription?.Length > 500, $"{prefix}.{nameof(translation.MetaDescription)}", "Meta description cannot exceed 500 characters.");
        }
    }
}
