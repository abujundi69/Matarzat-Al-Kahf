using MatarzatAlKahf.Application.Catalog.Categories;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MatarzatAlKahf.Infrastructure.Catalog.Categories;
public sealed partial class AdminCategoryService
{
    public async Task<AdminCategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        _validator.Validate(request);
        try
        {
            Guid id = await DbTransactionExecutor.ExecuteAsync(_dbContext, async token =>
            {
                await EnsureParentExistsAsync(request.ParentCategoryId, token);
                Dictionary<LanguageCode, string> slugs = NormalizeSlugs(request.Translations);
                await EnsureUniqueSlugsAsync(slugs, null, token);
                var category = new Category
                {
                    ParentCategoryId = request.ParentCategoryId,
                    ImageUrl = request.ImageUrl?.Trim() ?? string.Empty,
                    SortOrder = request.SortOrder,
                    IsActive = request.IsActive
                };
                foreach (CategoryTranslationRequest translation in request.Translations)
                {
                    category.Translations.Add(CreateTranslation(translation, slugs[translation.Language]));
                }

                _dbContext.Categories.Add(category);
                await _dbContext.SaveChangesAsync(token);
                return category.Id;
            }, cancellationToken);
            LogCategoryCreated(_logger, id, null);
            return await GetByIdAsync(id, cancellationToken);
        }
        catch (DbUpdateException exception)when (DbUpdateExceptionTranslator.IsUniqueViolation(exception))
        {
            throw new ConflictException("A category slug already exists for this language.", exception);
        }
    }
}
