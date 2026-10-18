using MatarzatAlKahf.Domain.Common;

namespace MatarzatAlKahf.Domain.Entities.System;

public sealed class StoreSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;

    public string? Value { get; set; }

    public string Group { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsPublic { get; set; }

    public bool IsLocalized { get; set; }

    public ICollection<StoreSettingTranslation> Translations { get; set; } = new List<StoreSettingTranslation>();
}
