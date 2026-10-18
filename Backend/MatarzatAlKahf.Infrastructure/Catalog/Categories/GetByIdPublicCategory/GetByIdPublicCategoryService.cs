using MatarzatAlKahf.Application.Catalog.Categories;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Catalog.Categories;
public sealed partial class PublicCategoryService
{
    public async Task<PublicCategoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        LanguageCode language = _languageResolver.GetCurrentLanguage();
        CategoryReadRow? row = await Project(AvailableCategories().Where(category => category.Id == id), language).SingleOrDefaultAsync(cancellationToken);
        return row is null ? throw new NotFoundException(nameof(Category), id) : Map(row);
    }
}
