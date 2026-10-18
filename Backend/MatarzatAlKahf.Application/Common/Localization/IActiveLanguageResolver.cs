using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Common.Localization;

public interface IActiveLanguageResolver
{
    LanguageCode GetCurrentLanguage();
}
