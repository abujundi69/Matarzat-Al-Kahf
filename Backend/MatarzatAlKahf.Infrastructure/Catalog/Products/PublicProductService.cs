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
public sealed partial class PublicProductService(MatarzatAlKahfDbContext dbContext, IActiveLanguageResolver languageResolver, ISlugNormalizer slugNormalizer, IProductRequestValidator validator) : IPublicProductService
{
    private readonly MatarzatAlKahfDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly IActiveLanguageResolver _languageResolver = languageResolver ?? throw new ArgumentNullException(nameof(languageResolver));
    private readonly ISlugNormalizer _slugNormalizer = slugNormalizer ?? throw new ArgumentNullException(nameof(slugNormalizer));
    private readonly IProductRequestValidator _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private sealed record ProductListBaseRow(Guid Id, Guid CategoryId, decimal BasePrice, decimal MinimumEffectivePrice, CurrencyCode Currency, bool IsFeatured, DateTime? PublishedAtUtc, bool InStock);
    private sealed record ProductListTranslationRow(Guid ProductId, LanguageCode Language, string Name, string Slug);
    private sealed record CategoryTranslationRow(Guid CategoryId, LanguageCode Language, string Name, string Slug);
    private sealed record ProductListImageRow(Guid Id, Guid ProductId, string ImageUrl, bool IsPrimary, int SortOrder);
    private sealed record ImageTranslationRow(Guid ImageId, LanguageCode Language, string AltText);
    private sealed record ProductDetailsBaseRow(Guid Id, Guid CategoryId, decimal BasePrice, CurrencyCode Currency, bool IsFeatured, DateTime? PublishedAtUtc);
    private sealed record ProductDetailsTranslationRow(LanguageCode Language, string Name, string Slug, string ShortDescription, string Description, string Story, string MetaTitle, string MetaDescription);
    private sealed record ProductDetailsImageRow(Guid Id, string ImageUrl, string ThumbnailUrl, int SortOrder, bool IsPrimary);
}
