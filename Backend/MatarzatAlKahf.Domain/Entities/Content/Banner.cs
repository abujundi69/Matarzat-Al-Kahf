using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Content;

public sealed class Banner : BaseEntity
{
    public BannerPosition Position { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime? StartsAtUtc { get; set; }

    public DateTime? EndsAtUtc { get; set; }

    public ICollection<BannerTranslation> Translations { get; set; } = new List<BannerTranslation>();
}
