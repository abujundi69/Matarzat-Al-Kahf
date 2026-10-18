using MatarzatAlKahf.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace MatarzatAlKahf.Api.Errors;

internal sealed partial class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHostEnvironment _environment =
        environment ?? throw new ArgumentNullException(nameof(environment));

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        // A client that navigates away mid-request aborts the connection, which surfaces here
        // as a cancellation. That is not a server fault, and the aborted response stream cannot
        // be written to, so claim the exception instead of logging a 500 and then failing again
        // inside the error writer.
        if (httpContext.RequestAborted.IsCancellationRequested)
        {
            LogRequestAborted(_logger, httpContext.TraceIdentifier);
            return true;
        }

        (int status, string title, string detail, IReadOnlyDictionary<string, string[]>? errors) =
            MapException(exception);

        if (status == StatusCodes.Status500InternalServerError)
        {
            LogUnhandledRequestFailure(
                _logger,
                exception,
                httpContext.TraceIdentifier);
        }
        else
        {
            LogRejectedRequest(
                _logger,
                httpContext.Request.Method,
                httpContext.Request.Path.Value ?? string.Empty,
                status,
                exception.GetType().Name,
                httpContext.TraceIdentifier);
        }

        try
        {
            await ApiProblemDetailsWriter.WriteAsync(
                httpContext,
                status,
                title,
                detail,
                errors,
                cancellationToken);
        }
        catch (Exception writeFailure)
        {
            // Reported here rather than left to the hosting middleware, so this handler is the
            // single place that records every request failure and the framework's duplicate
            // error-level entry can be filtered out of deployment logs.
            LogResponseWriteFailure(
                _logger,
                writeFailure,
                httpContext.TraceIdentifier);
        }

        return true;
    }

    private (int Status, string Title, string Detail,
        IReadOnlyDictionary<string, string[]>? Errors) MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException =>
                (StatusCodes.Status400BadRequest,
                    "Validation error",
                    validationException.Message,
                    validationException.Errors),
            AuthenticationFailedException authenticationException =>
                (StatusCodes.Status401Unauthorized,
                    "Authentication failed",
                    authenticationException.Message,
                    null),
            AuthorizationDeniedException authorizationException =>
                (StatusCodes.Status403Forbidden,
                    "Access denied",
                    authorizationException.Message,
                    null),
            NotFoundException notFoundException =>
                (StatusCodes.Status404NotFound,
                    "Resource not found",
                    notFoundException.Message,
                    null),
            ConcurrencyConflictException concurrencyException =>
                (StatusCodes.Status409Conflict,
                    "Concurrency conflict",
                    concurrencyException.Message,
                    null),
            ConflictException conflictException =>
                (StatusCodes.Status409Conflict,
                    "Conflict",
                    conflictException.Message,
                    null),
            RateLimitExceededException rateLimitException =>
                (StatusCodes.Status429TooManyRequests,
                    "Rate limit exceeded",
                    rateLimitException.Message,
                    null),
            ServiceUnavailableException unavailableException =>
                (StatusCodes.Status503ServiceUnavailable,
                    "Service unavailable",
                    unavailableException.Message,
                    null),
            _ =>
                (StatusCodes.Status500InternalServerError,
                    "Server error",
                    _environment.IsDevelopment()
                        ? "An unexpected server error occurred. See server logs using the trace identifier."
                        : "An unexpected server error occurred.",
                    null)
        };
    }

    [LoggerMessage(
        EventId = 2000,
        Level = LogLevel.Error,
        Message = "Unhandled request failure for trace {TraceId}")]
    private static partial void LogUnhandledRequestFailure(
        ILogger logger,
        Exception exception,
        string traceId);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Error,
        Message = "Writing the error response failed for trace {TraceId}")]
    private static partial void LogResponseWriteFailure(
        ILogger logger,
        Exception exception,
        string traceId);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Debug,
        Message = "Request aborted by the client for trace {TraceId}")]
    private static partial void LogRequestAborted(
        ILogger logger,
        string traceId);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Warning,
        Message = "Request {Method} {Path} rejected with status {StatusCode} and error type {ErrorType} for trace {TraceId}")]
    private static partial void LogRejectedRequest(
        ILogger logger,
        string method,
        string path,
        int statusCode,
        string errorType,
        string traceId);
}
