using MatarzatAlKahf.Domain.Common;

namespace MatarzatAlKahf.Domain.Entities.System;

public sealed class SeoSetting : BaseEntity
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ArabicTitle { get; set; } = string.Empty;
    public string EnglishTitle { get; set; } = string.Empty;
    public string ArabicDescription { get; set; } = string.Empty;
    public string EnglishDescription { get; set; } = string.Empty;
    public string ArabicOpenGraphImageUrl { get; set; } = string.Empty;
    public string EnglishOpenGraphImageUrl { get; set; } = string.Empty;
    public string RobotsAdditionalRules { get; set; } = string.Empty;
}
