using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Catalog;

public sealed class ProductImageTranslation : BaseEntity
{
    public Guid ProductImageId { get; set; }

    public LanguageCode Language { get; set; }

    public string AltText { get; set; } = string.Empty;

    public ProductImage ProductImage { get; set; } = null!;
}
