using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Catalog;

public sealed class ProductTranslation : BaseEntity
{
    public Guid ProductId { get; set; }

    public LanguageCode Language { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string ShortDescription { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Story { get; set; } = string.Empty;

    public string MetaTitle { get; set; } = string.Empty;

    public string MetaDescription { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
}
