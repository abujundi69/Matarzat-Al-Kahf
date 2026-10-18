using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public sealed record UpdateProductVariantRequest(Guid? Id, string Sku, string? Size, string? Color, decimal? WeightInGrams, string? Capacity, decimal? PriceOverride, bool IsActive, int? InitialQuantity, int ReorderLevel);
