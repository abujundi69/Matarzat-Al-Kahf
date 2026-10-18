using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.System;

public sealed class NotificationTemplate : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }
    public string ArabicTitle { get; set; } = string.Empty;
    public string EnglishTitle { get; set; } = string.Empty;
    public string ArabicBody { get; set; } = string.Empty;
    public string EnglishBody { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
