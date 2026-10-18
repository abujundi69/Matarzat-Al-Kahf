using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public sealed record PublicProductVariantDto(Guid Id, string Sku, string? Size, string? Color, decimal? WeightInGrams, string? Capacity, decimal EffectivePrice, bool InStock, int AvailableQuantity);
