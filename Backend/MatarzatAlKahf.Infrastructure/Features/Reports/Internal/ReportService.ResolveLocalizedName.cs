using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Infrastructure.Features.Reports.Services;

public sealed partial class ReportService
{
    internal LanguageCode ActiveLanguage => languageResolver.GetCurrentLanguage();

    internal static string ResolveLocalizedName(
        string? requestedName,
        string? arabicFallbackName,
        string finalFallback)
    {
        if (!string.IsNullOrWhiteSpace(requestedName))
        {
            return requestedName;
        }

        return string.IsNullOrWhiteSpace(arabicFallbackName)
            ? finalFallback
            : arabicFallbackName;
    }
}
