using MatarzatAlKahf.Application.Catalog.Categories;
using MatarzatAlKahf.Application.Catalog.Products;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Inventory;
using Microsoft.Extensions.DependencyInjection;

namespace MatarzatAlKahf.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ISlugNormalizer, SlugNormalizer>();
        services.AddSingleton<IPhoneNumberNormalizer, PhoneNumberNormalizer>();
        services.AddSingleton<ISkuNormalizer, SkuNormalizer>();
        services.AddSingleton<IPasswordPolicy, StrongPasswordPolicy>();
        services.AddSingleton<ICategoryRequestValidator, CategoryRequestValidator>();
        services.AddSingleton<IProductRequestValidator, ProductRequestValidator>();
        services.AddSingleton<IInventoryAdjustmentValidator, InventoryAdjustmentValidator>();

        return services;
    }
}
