using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Content;

public sealed class ContentPage : BaseEntity
{
    public ContentPageType Type { get; set; }

    public bool IsPublished { get; set; }

    public ICollection<ContentPageTranslation> Translations { get; set; } = new List<ContentPageTranslation>();
}
