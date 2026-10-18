using System.Globalization;
using MatarzatAlKahf.Application.Common.Localization;
using Microsoft.AspNetCore.Localization;

namespace MatarzatAlKahf.Api.Localization;

internal static class LocalizationServiceCollectionExtensions
{
    private static readonly string[] DefaultSupportedCultures = ["ar", "en"];

    public static IServiceCollection AddApiLocalization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddLocalization(options => options.ResourcesPath = "Resources");

        string defaultCulture = configuration["Localization:DefaultCulture"] ?? "ar";
        string[] supportedCultureNames = configuration
            .GetSection("Localization:SupportedCultures")
            .Get<string[]>() ?? DefaultSupportedCultures;

        ValidateCultures(defaultCulture, supportedCultureNames);

        CultureInfo[] supportedCultures = supportedCultureNames
            .Select(CultureInfo.GetCultureInfo)
            .ToArray();

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture(defaultCulture);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });

        return services;
    }

    public static IServiceCollection AddCurrentLanguage(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpContextAccessor();
        services.AddScoped<IActiveLanguageResolver, HttpActiveLanguageResolver>();

        return services;
    }

    private static void ValidateCultures(
        string defaultCulture,
        string[] supportedCultures)
    {
        if (supportedCultures.Length == 0)
        {
            throw new InvalidOperationException(
                "Localization:SupportedCultures must contain at least one culture.");
        }

        if (!supportedCultures.Contains(defaultCulture, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Localization:DefaultCulture must be included in Localization:SupportedCultures.");
        }

        if (supportedCultures.Any(culture =>
                !DefaultSupportedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "Only the 'ar' and 'en' request cultures are supported.");
        }
    }
}
