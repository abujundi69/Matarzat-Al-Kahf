namespace MatarzatAlKahf.Api.Errors;

internal static class ProblemDetailsServiceCollectionExtensions
{
    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
                context.ProblemDetails.Extensions.TryAdd(
                    "traceId",
                    context.HttpContext.TraceIdentifier);
        });
        services.AddExceptionHandler<ApiExceptionHandler>();

        return services;
    }
}
