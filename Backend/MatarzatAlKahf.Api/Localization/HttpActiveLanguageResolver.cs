using System.Globalization;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Api.Localization;

internal sealed class HttpActiveLanguageResolver : IActiveLanguageResolver
{
    public LanguageCode GetCurrentLanguage()
    {
        return string.Equals(
            CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
            "en",
            StringComparison.OrdinalIgnoreCase)
            ? LanguageCode.English
            : LanguageCode.Arabic;
    }
}
