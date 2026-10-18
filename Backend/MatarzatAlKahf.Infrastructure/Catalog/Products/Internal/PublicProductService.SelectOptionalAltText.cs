using MatarzatAlKahf.Application.Catalog.Products;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Catalog.Products;
public sealed partial class PublicProductService
{
    private static string? SelectOptionalAltText(Guid imageId, List<ImageTranslationRow> translations, LanguageCode language)
    {
        ImageTranslationRow? translation = translations.Where(value => value.ImageId == imageId).OrderBy(value => LanguagePriority(value.Language, language)).FirstOrDefault();
        return EmptyToNull(translation?.AltText);
    }
}
