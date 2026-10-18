using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Content;

public sealed class ContentPageTranslation : BaseEntity
{
    public Guid ContentPageId { get; set; }

    public LanguageCode Language { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string MetaTitle { get; set; } = string.Empty;

    public string MetaDescription { get; set; } = string.Empty;

    public ContentPage ContentPage { get; set; } = null!;
}
