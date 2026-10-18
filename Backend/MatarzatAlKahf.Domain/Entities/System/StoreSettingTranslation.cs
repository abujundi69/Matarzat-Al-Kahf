using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.System;

public sealed class StoreSettingTranslation : BaseEntity
{
    public Guid StoreSettingId { get; set; }

    public LanguageCode Language { get; set; }

    public string Value { get; set; } = string.Empty;

    public StoreSetting StoreSetting { get; set; } = null!;
}
