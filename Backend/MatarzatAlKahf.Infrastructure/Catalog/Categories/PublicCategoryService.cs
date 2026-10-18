using MatarzatAlKahf.Application.Catalog.Categories;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Catalog.Categories;
public sealed partial class PublicCategoryService(MatarzatAlKahfDbContext dbContext, IActiveLanguageResolver languageResolver, ISlugNormalizer slugNormalizer) : IPublicCategoryService
{
    private readonly MatarzatAlKahfDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly IActiveLanguageResolver _languageResolver = languageResolver ?? throw new ArgumentNullException(nameof(languageResolver));
    private readonly ISlugNormalizer _slugNormalizer = slugNormalizer ?? throw new ArgumentNullException(nameof(slugNormalizer));
    private sealed record CategoryReadRow(Guid Id, Guid? ParentCategoryId, string ImageUrl, int SortOrder, int ProductCount, TranslationReadRow Translation, IReadOnlyList<ChildReadRow> Children);
    private sealed record TranslationReadRow(string Name, string Slug, string Description, string MetaTitle, string MetaDescription);
    private sealed record ChildReadRow(Guid Id, Guid? ParentCategoryId, ChildTranslationReadRow Translation);
    private sealed record ChildTranslationReadRow(string Name, string Slug);
}
