using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Domain.Enums;
using Microsoft.Net.Http.Headers;

namespace MatarzatAlKahf.Api.Localization;

internal sealed class ContentLanguageMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        IActiveLanguageResolver languageResolver)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(languageResolver);

        LanguageCode language = languageResolver.GetCurrentLanguage();
        string languageCode = language == LanguageCode.English ? "en" : "ar";

        context.Response.OnStarting(
            static state =>
            {
                var (httpContext, contentLanguage) =
                    ((HttpContext HttpContext, string ContentLanguage))state;

                if (!httpContext.Response.Headers.ContainsKey(HeaderNames.ContentLanguage))
                {
                    httpContext.Response.Headers.ContentLanguage = contentLanguage;
                }

                return Task.CompletedTask;
            },
            (context, languageCode));

        await _next(context);
    }
}
