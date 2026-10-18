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
    public async Task<AdminProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        _validator.Validate(request);
        var removedImageUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            await DbTransactionExecutor.ExecuteAsync(_dbContext, async token =>
            {
                Product product = await ProductGraph(asNoTracking: false).SingleOrDefaultAsync(entity => entity.Id == id && !entity.IsDeleted, token) ?? throw new NotFoundException(nameof(Product), id);
                await EnsureActiveCategoryAsync(request.CategoryId, token);
                Dictionary<LanguageCode, string> slugs = NormalizeSlugs(request.Translations);
                await EnsureUniqueSlugsAsync(slugs, id, token);
                SkuCandidate[] skuCandidates = request.Variants.Select(variant => new SkuCandidate(variant.Id, _skuNormalizer.Normalize(variant.Sku))).ToArray();
                await EnsureUniqueSkusAsync(skuCandidates, token);
                product.CategoryId = request.CategoryId;
                product.BasePrice = request.BasePrice;
                product.Currency = request.Currency;
                product.Status = request.Status;
                product.IsFeatured = request.IsFeatured;
                product.PublishedAtUtc = request.PublishedAtUtc;
                product.UpdatedAtUtc = DateTime.UtcNow;
                UpdateTranslations(product, request.Translations, slugs);
                UpdateVariants(product, request.Variants);
                if (request.Images is not null)
                {
                    string[] previousImageUrls = product.Images
                        .Where(image => !image.IsDeleted)
                        .SelectMany(image => new[] { image.ImageUrl, image.ThumbnailUrl })
                        .Where(url => !string.IsNullOrWhiteSpace(url))
                        .ToArray();
                    UpdateImages(product, request.Images);
                    var retainedImageUrls = product.Images
                        .Where(image => !image.IsDeleted)
                        .SelectMany(image => new[] { image.ImageUrl, image.ThumbnailUrl })
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
                    removedImageUrls.UnionWith(
                        previousImageUrls.Where(url => !retainedImageUrls.Contains(url)));
                }

                await _dbContext.SaveChangesAsync(token);
            }, cancellationToken);
            foreach (string removedImageUrl in removedImageUrls)
            {
                await _fileCleanup.DeleteIfUnreferencedAsync(
                    removedImageUrl,
                    cancellationToken);
            }

            LogProductUpdated(_logger, id, null);
            return await GetByIdAsync(id, cancellationToken);
        }
        catch (DbUpdateException exception)when (DbUpdateExceptionTranslator.IsUniqueViolation(exception))
        {
            throw new ConflictException("A product slug or SKU already exists.", exception);
        }
    }
}
