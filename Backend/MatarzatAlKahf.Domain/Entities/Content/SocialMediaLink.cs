using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Content;

public sealed class SocialMediaLink : BaseEntity
{
    public SocialPlatform Platform { get; set; }

    public string Url { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }
}
