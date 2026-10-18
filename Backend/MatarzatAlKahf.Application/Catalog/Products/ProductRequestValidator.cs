using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Validation;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public sealed class ProductRequestValidator(ISlugNormalizer slugNormalizer, ISkuNormalizer skuNormalizer) : IProductRequestValidator
{
    private readonly ISlugNormalizer _slugNormalizer = slugNormalizer ?? throw new ArgumentNullException(nameof(slugNormalizer));
    private readonly ISkuNormalizer _skuNormalizer = skuNormalizer ?? throw new ArgumentNullException(nameof(skuNormalizer));
    public void Validate(CreateProductRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var errors = CreateMainProductErrors(request.CategoryId, request.BasePrice, request.Currency, request.Status, request.Translations);
        ValidateCreateVariants(request.Variants, request.Status, errors);
        ValidateCreateImages(request.Images, errors);
        ValidationRuleUtilities.ThrowIfAny(errors, "Product is invalid.");
    }

    public void Validate(UpdateProductRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var errors = CreateMainProductErrors(request.CategoryId, request.BasePrice, request.Currency, request.Status, request.Translations);
        ValidateUpdateVariants(request.Variants, request.Status, errors);
        ValidateUpdateImages(request.Images, errors);
        ValidationRuleUtilities.ThrowIfAny(errors, "Product is invalid.");
    }

    public void Validate(ProductListQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        PaginationRules.Validate(query.Page, query.PageSize);
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        ValidationRuleUtilities.AddIf(errors, query.MinPrice < 0, nameof(query.MinPrice), "Minimum price cannot be negative.");
        ValidationRuleUtilities.AddIf(errors, query.MaxPrice < 0, nameof(query.MaxPrice), "Maximum price cannot be negative.");
        ValidationRuleUtilities.AddIf(errors, query.MinPrice.HasValue && query.MaxPrice.HasValue && query.MinPrice.Value > query.MaxPrice.Value, nameof(query.MaxPrice), "Maximum price must be greater than or equal to minimum price.");
        ValidationRuleUtilities.AddIf(errors, string.IsNullOrWhiteSpace(query.Sort) || !ProductSortValues.All.Contains(query.Sort, StringComparer.OrdinalIgnoreCase), nameof(query.Sort), "Sort must be newest, price-asc, price-desc, or name.");
        ValidationRuleUtilities.ThrowIfAny(errors, "Product query is invalid.");
    }

    private Dictionary<string, List<string>> CreateMainProductErrors(Guid categoryId, decimal basePrice, CurrencyCode currency, ProductStatus status, IReadOnlyList<ProductTranslationRequest> translations)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        ValidationRuleUtilities.AddIf(errors, categoryId == Guid.Empty, nameof(categoryId), "Category is required.");
        ValidationRuleUtilities.AddIf(errors, basePrice < 0, nameof(basePrice), "Base price cannot be negative.");
        ValidationRuleUtilities.AddIf(errors, !Enum.IsDefined(currency), nameof(currency), "Currency is not supported.");
        ValidationRuleUtilities.AddIf(errors, Enum.IsDefined(currency) && currency != CurrencyCode.ILS, nameof(currency), "Active commerce supports ILS only.");
        ValidationRuleUtilities.AddIf(errors, !Enum.IsDefined(status), nameof(status), "Product status is not supported.");
        ValidateTranslations(translations, errors);
        return errors;
    }

    private void ValidateTranslations(IReadOnlyList<ProductTranslationRequest> translations, Dictionary<string, List<string>> errors)
    {
        if (translations is null)
        {
            ValidationRuleUtilities.Add(errors, nameof(translations), "Arabic and English translations are required.");
            return;
        }

        ValidationRuleUtilities.ValidateRequiredLanguages(translations.Select(translation => translation.Language), nameof(translations), errors);
        for (int index = 0; index < translations.Count; index++)
        {
            ProductTranslationRequest translation = translations[index];
            string prefix = $"{nameof(translations)}[{index}]";
            ValidateRequiredText(translation.Name, 200, $"{prefix}.{nameof(translation.Name)}", errors);
            if (string.IsNullOrWhiteSpace(translation.Slug))
            {
                ValidationRuleUtilities.Add(errors, $"{prefix}.{nameof(translation.Slug)}", "Product slug is required.");
            }
            else
            {
                string normalizedSlug = _slugNormalizer.Normalize(translation.Slug);
                ValidationRuleUtilities.AddIf(errors, normalizedSlug.Length > 250, $"{prefix}.{nameof(translation.Slug)}", "Product slug cannot exceed 250 characters.");
            }

            ValidateRequiredText(translation.Description, 4000, $"{prefix}.{nameof(translation.Description)}", errors);
            ValidateOptionalText(translation.ShortDescription, 1000, $"{prefix}.{nameof(translation.ShortDescription)}", errors);
            ValidateOptionalText(translation.MetaTitle, 250, $"{prefix}.{nameof(translation.MetaTitle)}", errors);
            ValidateOptionalText(translation.MetaDescription, 500, $"{prefix}.{nameof(translation.MetaDescription)}", errors);
        }
    }

    private void ValidateCreateVariants(IReadOnlyList<CreateProductVariantRequest> variants, ProductStatus status, Dictionary<string, List<string>> errors)
    {
        if (variants is null || variants.Count == 0)
        {
            ValidationRuleUtilities.Add(errors, nameof(variants), "A product must contain at least one variant.");
            return;
        }

        ValidateUniqueSkus(variants.Select(variant => variant.Sku), errors);
        for (int index = 0; index < variants.Count; index++)
        {
            CreateProductVariantRequest variant = variants[index];
            ValidateVariant(index, variant.Sku, variant.Size, variant.Color, variant.WeightInGrams, variant.Capacity, variant.PriceOverride, variant.InitialQuantity, variant.ReorderLevel, errors);
        }

        if (status == ProductStatus.Active && variants.All(variant => !variant.IsActive))
        {
            ValidationRuleUtilities.Add(errors, nameof(variants), "An active product must contain at least one active variant.");
        }
    }

    private void ValidateUpdateVariants(IReadOnlyList<UpdateProductVariantRequest> variants, ProductStatus status, Dictionary<string, List<string>> errors)
    {
        if (variants is null || variants.Count == 0)
        {
            ValidationRuleUtilities.Add(errors, nameof(variants), "A product must contain at least one variant.");
            return;
        }

        ValidateUniqueSkus(variants.Select(variant => variant.Sku), errors);
        ValidateUniqueChildIds(variants.Select(variant => variant.Id), nameof(variants), errors);
        for (int index = 0; index < variants.Count; index++)
        {
            UpdateProductVariantRequest variant = variants[index];
            int initialQuantity = variant.InitialQuantity ?? 0;
            ValidateVariant(index, variant.Sku, variant.Size, variant.Color, variant.WeightInGrams, variant.Capacity, variant.PriceOverride, initialQuantity, variant.ReorderLevel, errors);
            ValidationRuleUtilities.AddIf(errors, variant.Id.HasValue && variant.InitialQuantity.HasValue, $"{nameof(variants)}[{index}].{nameof(variant.InitialQuantity)}", "Inventory quantity cannot be overwritten for an existing variant.");
        }

        if (status == ProductStatus.Active && variants.All(variant => !variant.IsActive))
        {
            ValidationRuleUtilities.Add(errors, nameof(variants), "An active product must contain at least one active variant.");
        }
    }

    private void ValidateVariant(int index, string sku, string? size, string? color, decimal? weightInGrams, string? capacity, decimal? priceOverride, int initialQuantity, int reorderLevel, Dictionary<string, List<string>> errors)
    {
        string prefix = $"variants[{index}]";
        if (string.IsNullOrWhiteSpace(sku))
        {
            ValidationRuleUtilities.Add(errors, $"{prefix}.sku", "SKU is required.");
        }
        else
        {
            _skuNormalizer.Normalize(sku);
        }

        ValidateOptionalText(size, 100, $"{prefix}.size", errors);
        ValidateOptionalText(color, 100, $"{prefix}.color", errors);
        ValidateOptionalText(capacity, 100, $"{prefix}.capacity", errors);
        ValidationRuleUtilities.AddIf(errors, weightInGrams < 0, $"{prefix}.weightInGrams", "Weight cannot be negative.");
        ValidationRuleUtilities.AddIf(errors, priceOverride < 0, $"{prefix}.priceOverride", "Price override cannot be negative.");
        ValidationRuleUtilities.AddIf(errors, initialQuantity < 0, $"{prefix}.initialQuantity", "Initial inventory quantity cannot be negative.");
        ValidationRuleUtilities.AddIf(errors, reorderLevel < 0, $"{prefix}.reorderLevel", "Reorder level cannot be negative.");
    }

    private void ValidateUniqueSkus(IEnumerable<string> skus, Dictionary<string, List<string>> errors)
    {
        string[] normalizedSkus = skus.Where(sku => !string.IsNullOrWhiteSpace(sku)).Select(_skuNormalizer.Normalize).ToArray();
        if (normalizedSkus.Distinct(StringComparer.Ordinal).Count() != normalizedSkus.Length)
        {
            ValidationRuleUtilities.Add(errors, "variants", "Duplicate SKUs are not allowed in the same request.");
        }
    }

    private static void ValidateCreateImages(IReadOnlyList<CreateProductImageRequest>? images, Dictionary<string, List<string>> errors)
    {
        if (images is null)
        {
            return;
        }

        ValidateImages(images.Select(image => new ImageValidationValues(image.ImageUrl, image.ThumbnailUrl, image.SortOrder, image.IsPrimary, image.Translations)), errors);
    }

    private static void ValidateUpdateImages(IReadOnlyList<UpdateProductImageRequest>? images, Dictionary<string, List<string>> errors)
    {
        if (images is null)
        {
            return;
        }

        ValidateUniqueChildIds(images.Select(image => image.Id), nameof(images), errors);
        ValidateImages(images.Select(image => new ImageValidationValues(image.ImageUrl, image.ThumbnailUrl, image.SortOrder, image.IsPrimary, image.Translations)), errors);
    }

    private static void ValidateImages(IEnumerable<ImageValidationValues> imageValues, Dictionary<string, List<string>> errors)
    {
        ImageValidationValues[] images = imageValues.ToArray();
        if (images.Count(image => image.IsPrimary) > 1)
        {
            ValidationRuleUtilities.Add(errors, "images", "Only one image may be primary for a product.");
        }

        for (int index = 0; index < images.Length; index++)
        {
            ImageValidationValues image = images[index];
            string prefix = $"images[{index}]";
            ValidateImageUrl(image.ImageUrl, $"{prefix}.imageUrl", errors);
            ValidateImageUrl(image.ThumbnailUrl, $"{prefix}.thumbnailUrl", errors);
            ValidationRuleUtilities.AddIf(errors, image.SortOrder < 0, $"{prefix}.sortOrder", "Image sort order cannot be negative.");
            ValidateImageTranslations(image.Translations, prefix, errors);
        }
    }

    private static void ValidateImageTranslations(IReadOnlyList<ProductImageTranslationRequest> translations, string prefix, Dictionary<string, List<string>> errors)
    {
        if (translations is null)
        {
            return;
        }

        LanguageCode[] languages = translations.Select(translation => translation.Language).ToArray();
        if (languages.Distinct().Count() != languages.Length)
        {
            ValidationRuleUtilities.Add(errors, $"{prefix}.translations", "An image can contain only one translation per language.");
        }

        for (int index = 0; index < translations.Count; index++)
        {
            ProductImageTranslationRequest translation = translations[index];
            ValidationRuleUtilities.AddIf(errors, !Enum.IsDefined(translation.Language), $"{prefix}.translations[{index}].language", "Image translation language is not supported.");
            ValidateOptionalText(translation.AltText, 500, $"{prefix}.translations[{index}].altText", errors);
        }
    }

    private static void ValidateImageUrl(string value, string field, Dictionary<string, List<string>> errors)
    {
        bool isValid = !string.IsNullOrWhiteSpace(value) && value.Length <= 2000 && (IsHttpUrl(value) || IsRootRelativeUrl(value));
        ValidationRuleUtilities.AddIf(errors, !isValid, field, "Image URL must be an HTTP(S) URL or a root-relative URL.");
    }

    private static bool IsHttpUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static bool IsRootRelativeUrl(string value)
    {
        return value.StartsWith('/') && !value.StartsWith("//", StringComparison.Ordinal) && Uri.TryCreate(value, UriKind.Relative, out _);
    }

    private static void ValidateUniqueChildIds(IEnumerable<Guid?> ids, string field, Dictionary<string, List<string>> errors)
    {
        Guid[] values = ids.Where(id => id.HasValue).Select(id => id!.Value).ToArray();
        if (values.Distinct().Count() != values.Length)
        {
            ValidationRuleUtilities.Add(errors, field, "Duplicate child identifiers are not allowed.");
        }
    }

    private static void ValidateRequiredText(string? value, int maximumLength, string field, Dictionary<string, List<string>> errors)
    {
        ValidationRuleUtilities.AddIf(errors, string.IsNullOrWhiteSpace(value), field, "A value is required.");
        ValidateOptionalText(value, maximumLength, field, errors);
    }

    private static void ValidateOptionalText(string? value, int maximumLength, string field, Dictionary<string, List<string>> errors)
    {
        ValidationRuleUtilities.AddIf(errors, value?.Length > maximumLength, field, $"Value cannot exceed {maximumLength} characters.");
    }

    private sealed record ImageValidationValues(string ImageUrl, string ThumbnailUrl, int SortOrder, bool IsPrimary, IReadOnlyList<ProductImageTranslationRequest> Translations);
}
