namespace MatarzatAlKahf.Api.Security;

/// <summary>
/// Applies the baseline response headers every API response should carry. The API serves JSON
/// and authenticated downloads, so the goal is to stop content sniffing, framing, and referrer
/// leakage rather than to govern page rendering.
/// </summary>
internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    private const string UploadsPrefix = "/uploads";

    private readonly RequestDelegate _next =
        next ?? throw new ArgumentNullException(nameof(next));

    public Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Public product imagery is embedded by the storefront, which may be served from a
        // different origin. Isolation headers belong on the API surface, not on those assets;
        // the static file handler already marks them "nosniff".
        bool isPublicUpload = context.Request.Path.StartsWithSegments(
            UploadsPrefix,
            StringComparison.OrdinalIgnoreCase);

        context.Response.OnStarting(
            static state =>
            {
                (HttpContext httpContext, bool publicUpload) = ((HttpContext, bool))state;
                IHeaderDictionary headers = httpContext.Response.Headers;
                headers.XContentTypeOptions = "nosniff";
                headers["Referrer-Policy"] = "no-referrer";
                if (publicUpload)
                {
                    return Task.CompletedTask;
                }

                headers.XFrameOptions = "DENY";
                // API responses are data, never documents, so forbidding every resource type
                // keeps an accidentally rendered response from loading or executing anything.
                headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
                return Task.CompletedTask;
            },
            (context, isPublicUpload));

        return _next(context);
    }
}
