using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Content;

public sealed class BannerTranslation : BaseEntity
{
    public Guid BannerId { get; set; }

    public LanguageCode Language { get; set; }

    public string Title { get; set; } = string.Empty;

    public string DesktopImageUrl { get; set; } = string.Empty;

    public string MobileImageUrl { get; set; } = string.Empty;

    public string AltText { get; set; } = string.Empty;

    public string LinkUrl { get; set; } = string.Empty;

    public Banner Banner { get; set; } = null!;
}
