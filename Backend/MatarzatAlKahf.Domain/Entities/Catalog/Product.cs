using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Catalog;

public sealed class Product : BaseEntity
{
    public Guid CategoryId { get; set; }

    public decimal BasePrice { get; set; }

    public CurrencyCode Currency { get; set; }

    public ProductStatus Status { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime? PublishedAtUtc { get; set; }

    public Category Category { get; set; } = null!;

    public ICollection<ProductTranslation> Translations { get; set; } = new List<ProductTranslation>();

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
