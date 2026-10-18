using MatarzatAlKahf.Application.Catalog.Products;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MatarzatAlKahf.Infrastructure.Catalog.Products;
public sealed partial class AdminProductService
{
    private static void UpdateTranslations(Product product, IEnumerable<ProductTranslationRequest> inputs, Dictionary<LanguageCode, string> slugs)
    {
        foreach (ProductTranslationRequest input in inputs)
        {
            ProductTranslation? translation = product.Translations.SingleOrDefault(entity => entity.Language == input.Language);
            if (translation is null)
            {
                product.Translations.Add(CreateTranslation(input, slugs[input.Language]));
                continue;
            }

            ApplyTranslation(translation, input, slugs[input.Language]);
        }
    }
}
