using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MatarzatAlKahf.Api.OpenApi;

internal static class OpenApiServiceCollectionExtensions
{
    public static IServiceCollection AddApiSwagger(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Matarzat Al Kahf API",
                Version = "v1",
                Description = "Phase One bilingual catalog and administrative API.",
            });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Description = "Enter a JWT access token. Swagger sends it as a Bearer token.",
            });
            options.OperationFilter<BearerSecurityOperationFilter>();
        });

        return services;
    }
}

internal sealed class BearerSecurityOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        Type? declaringType = context.MethodInfo.DeclaringType;
        bool allowsAnonymous = context.MethodInfo.IsDefined(
                typeof(AllowAnonymousAttribute),
                inherit: true)
            || declaringType?.IsDefined(typeof(AllowAnonymousAttribute), inherit: true) == true;
        bool requiresAuthorization = context.MethodInfo.IsDefined(
                typeof(AuthorizeAttribute),
                inherit: true)
            || declaringType?.IsDefined(typeof(AuthorizeAttribute), inherit: true) == true;

        operation.Summary ??= CreateSummary(context.MethodInfo);

        if (allowsAnonymous || !requiresAuthorization)
        {
            return;
        }

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                }] = Array.Empty<string>(),
            },
        ];
    }

    private static string CreateSummary(MethodInfo methodInfo)
    {
        string controller = methodInfo.DeclaringType?.Name
            .Replace("Controller", string.Empty, StringComparison.Ordinal) ?? "API";
        return $"{SplitPascalCase(methodInfo.Name)} ({SplitPascalCase(controller)})";
    }

    private static string SplitPascalCase(string value)
    {
        return string.Concat(
            value.Select((character, index) =>
                index > 0 && char.IsUpper(character)
                    ? $" {char.ToLowerInvariant(character)}"
                    : character.ToString()));
    }
}
