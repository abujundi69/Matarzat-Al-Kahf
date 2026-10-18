using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Catalog.Products;
public static class ProductSortValues
{
    public const string Newest = "newest";
    public const string PriceAscending = "price-asc";
    public const string PriceDescending = "price-desc";
    public const string Name = "name";
    public static IReadOnlyList<string> All { get; } = Array.AsReadOnly([Newest, PriceAscending, PriceDescending, Name]);
}
