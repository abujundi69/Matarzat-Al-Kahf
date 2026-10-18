using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Catalog;

public sealed class CategoryTranslation : BaseEntity
{
    public Guid CategoryId { get; set; }

    public LanguageCode Language { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string MetaTitle { get; set; } = string.Empty;

    public string MetaDescription { get; set; } = string.Empty;

    public Category Category { get; set; } = null!;
}
