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
    public async Task<AdminProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        _validator.Validate(request);
        try
        {
            Guid productId = await DbTransactionExecutor.ExecuteAsync(_dbContext, async token =>
            {
                await EnsureActiveCategoryAsync(request.CategoryId, token);
                Dictionary<LanguageCode, string> slugs = NormalizeSlugs(request.Translations);
                string[] skus = request.Variants.Select(variant => _skuNormalizer.Normalize(variant.Sku)).ToArray();
                await EnsureUniqueSlugsAsync(slugs, null, token);
                await EnsureUniqueSkusAsync(skus.Select(sku => new SkuCandidate(null, sku)), token);
                var product = new Product
                {
                    CategoryId = request.CategoryId,
                    BasePrice = request.BasePrice,
                    Currency = request.Currency,
                    Status = request.Status,
                    IsFeatured = request.IsFeatured,
                    PublishedAtUtc = request.PublishedAtUtc
                };
                foreach (ProductTranslationRequest translation in request.Translations)
                {
                    product.Translations.Add(CreateTranslation(translation, slugs[translation.Language]));
                }

                if (request.Images is not null)
                {
                    foreach (CreateProductImageRequest image in request.Images)
                    {
                        product.Images.Add(CreateImage(image));
                    }
                }

                for (int index = 0; index < request.Variants.Count; index++)
                {
                    CreateProductVariantRequest input = request.Variants[index];
                    product.Variants.Add(CreateVariant(input.Sku, input.Size, input.Color, input.WeightInGrams, input.Capacity, input.PriceOverride, input.IsActive, input.InitialQuantity, input.ReorderLevel));
                }

                _dbContext.Products.Add(product);
                await _dbContext.SaveChangesAsync(token);
                return product.Id;
            }, cancellationToken);
            LogProductCreated(_logger, productId, null);
            return await GetByIdAsync(productId, cancellationToken);
        }
        catch (DbUpdateException exception)when (DbUpdateExceptionTranslator.IsUniqueViolation(exception))
        {
            throw new ConflictException("A product slug or SKU already exists.", exception);
        }
    }
}
