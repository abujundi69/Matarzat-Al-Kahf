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
    public async Task<AdminCategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        _validator.Validate(request);
        string? previousImageUrl = null;
        try
        {
            await DbTransactionExecutor.ExecuteAsync(_dbContext, async token =>
            {
                Category category = await _dbContext.Categories.Include(entity => entity.Translations).SingleOrDefaultAsync(entity => entity.Id == id && !entity.IsDeleted, token) ?? throw new NotFoundException(nameof(Category), id);
                await EnsureParentExistsAsync(request.ParentCategoryId, token);
                IReadOnlyCollection<Guid> ancestorIds = await GetAncestorIdsAsync(request.ParentCategoryId, token);
                _validator.EnsureValidParent(id, request.ParentCategoryId, ancestorIds);
                Dictionary<LanguageCode, string> slugs = NormalizeSlugs(request.Translations);
                await EnsureUniqueSlugsAsync(slugs, id, token);
                category.ParentCategoryId = request.ParentCategoryId;
                if (request.ImageUrl is not null)
                {
                    previousImageUrl = category.ImageUrl;
                    category.ImageUrl = request.ImageUrl.Trim();
                }
                category.SortOrder = request.SortOrder;
                category.IsActive = request.IsActive;
                category.UpdatedAtUtc = DateTime.UtcNow;
                foreach (CategoryTranslationRequest input in request.Translations)
                {
                    CategoryTranslation? translation = category.Translations.SingleOrDefault(entity => entity.Language == input.Language);
                    if (translation is null)
                    {
                        category.Translations.Add(CreateTranslation(input, slugs[input.Language]));
                        continue;
                    }

                    ApplyTranslation(translation, input, slugs[input.Language]);
                }

                await _dbContext.SaveChangesAsync(token);
            }, cancellationToken);
            if (previousImageUrl is not null)
            {
                await _fileCleanup.DeleteIfUnreferencedAsync(
                    previousImageUrl,
                    cancellationToken);
            }

            LogCategoryUpdated(_logger, id, null);
            return await GetByIdAsync(id, cancellationToken);
        }
        catch (DbUpdateException exception)when (DbUpdateExceptionTranslator.IsUniqueViolation(exception))
        {
            throw new ConflictException("A category slug already exists for this language.", exception);
        }
    }
}
