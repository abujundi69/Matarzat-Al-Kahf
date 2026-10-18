using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Errors;

internal static class ApiProblemDetailsWriter
{
    public static Task WriteAsync(
        HttpContext httpContext,
        int statusCode,
        string title,
        string detail,
        IReadOnlyDictionary<string, string[]>? errors = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        ProblemDetails problemDetails = errors is null
            ? new ProblemDetails()
            : new ValidationProblemDetails(
                errors.ToDictionary(pair => pair.Key, pair => pair.Value));

        problemDetails.Status = statusCode;
        problemDetails.Title = title;
        problemDetails.Detail = detail;
        problemDetails.Instance = httpContext.Request.Path;
        problemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        CancellationToken effectiveCancellationToken = cancellationToken == default
            ? httpContext.RequestAborted
            : cancellationToken;

        return problemDetails is ValidationProblemDetails validationProblemDetails
            ? httpContext.Response.WriteAsJsonAsync(
                validationProblemDetails,
                options: null,
                contentType: "application/problem+json",
                cancellationToken: effectiveCancellationToken)
            : httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                options: null,
                contentType: "application/problem+json",
                cancellationToken: effectiveCancellationToken);
    }
}
