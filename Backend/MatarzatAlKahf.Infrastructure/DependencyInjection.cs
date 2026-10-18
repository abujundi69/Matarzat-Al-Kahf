using MatarzatAlKahf.Application.Authentication.Admin;
using MatarzatAlKahf.Application.Authentication.Unified;
using MatarzatAlKahf.Application.Catalog.Categories;
using MatarzatAlKahf.Application.Catalog.Products;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Features.AdminUsers.Interfaces;
using MatarzatAlKahf.Application.Features.AuditLogs.Interfaces;
using MatarzatAlKahf.Application.Features.Banners.Interfaces;
using MatarzatAlKahf.Application.Features.Carts.Interfaces;
using MatarzatAlKahf.Application.Features.Checkout.Interfaces;
using MatarzatAlKahf.Application.Features.ContactMessages.Interfaces;
using MatarzatAlKahf.Application.Features.ContentPages.Interfaces;
using MatarzatAlKahf.Application.Features.Coupons.Interfaces;
using MatarzatAlKahf.Application.Features.CustomerAddresses.Interfaces;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
using MatarzatAlKahf.Application.Features.Customers.Interfaces;
using MatarzatAlKahf.Application.Features.Dashboard.Interfaces;
using MatarzatAlKahf.Application.Features.DataImports.Interfaces;
using MatarzatAlKahf.Application.Features.Delivery.Interfaces;
using MatarzatAlKahf.Application.Features.Exports.Interfaces;
using MatarzatAlKahf.Application.Features.Files.Interfaces;
using MatarzatAlKahf.Application.Features.Home.Interfaces;
using MatarzatAlKahf.Application.Features.Notifications.Interfaces;
using MatarzatAlKahf.Application.Features.Orders.Interfaces;
using MatarzatAlKahf.Application.Features.Payments.Interfaces;
using MatarzatAlKahf.Application.Features.Reports.Interfaces;
using MatarzatAlKahf.Application.Features.RolesAndPermissions.Interfaces;
using MatarzatAlKahf.Application.Features.Seo.Interfaces;
using MatarzatAlKahf.Application.Features.SocialMediaLinks.Interfaces;
using MatarzatAlKahf.Application.Features.StoreSettings.Interfaces;
using MatarzatAlKahf.Application.Inventory;
using MatarzatAlKahf.Application.Maintenance;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Authentication.Admin;
using MatarzatAlKahf.Infrastructure.Authentication.Unified;
using MatarzatAlKahf.Infrastructure.Catalog.Categories;
using MatarzatAlKahf.Infrastructure.Catalog.Products;
using MatarzatAlKahf.Infrastructure.Features.AdminUsers.Services;
using MatarzatAlKahf.Infrastructure.Features.AuditLogs.Services;
using MatarzatAlKahf.Infrastructure.Features.Carts.Services;
using MatarzatAlKahf.Infrastructure.Features.Checkout.Services;
using MatarzatAlKahf.Infrastructure.Features.Content.Services;
using MatarzatAlKahf.Infrastructure.Features.Coupons.Services;
using MatarzatAlKahf.Infrastructure.Features.CustomerAddresses.Services;
using MatarzatAlKahf.Infrastructure.Features.CustomerAuthentication.Services;
using MatarzatAlKahf.Infrastructure.Features.Customers.Services;
using MatarzatAlKahf.Infrastructure.Features.Dashboard.Services;
using MatarzatAlKahf.Infrastructure.Features.DataImports.Services;
using MatarzatAlKahf.Infrastructure.Features.Delivery.Services;
using MatarzatAlKahf.Infrastructure.Features.Exports.Services;
using MatarzatAlKahf.Infrastructure.Features.Files.Services;
using MatarzatAlKahf.Infrastructure.Features.Notifications.Services;
using MatarzatAlKahf.Infrastructure.Features.Orders.Services;
using MatarzatAlKahf.Infrastructure.Features.Payments.Services;
using MatarzatAlKahf.Infrastructure.Features.Reports.Services;
using MatarzatAlKahf.Infrastructure.Features.RolesAndPermissions.Services;
using MatarzatAlKahf.Infrastructure.Features.Seo.Services;
using MatarzatAlKahf.Infrastructure.Inventory;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Development;
using MatarzatAlKahf.Infrastructure.Persistence.Maintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace MatarzatAlKahf.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "The SQL Server connection string 'DefaultConnection' is missing.");
        }

        services.AddDbContext<MatarzatAlKahfDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                sqlServerOptions =>
                {
                    sqlServerOptions.MigrationsAssembly("MatarzatAlKahf.Infrastructure");
                    sqlServerOptions.EnableRetryOnFailure();
                    // Aggregate loads (admin user -> roles -> permissions, orders, coupons) include
                    // several collections; one joined query would multiply their rows. Queries that
                    // need a single round trip can opt out with AsSingleQuery().
                    sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
            options.ConfigureDevelopmentSeed(configuration, environment);
        });

        services.AddSingleton<IAdminPasswordHasher, AdminPasswordHasher>();
        services.AddScoped<IAdminAuthenticationService, AdminAuthenticationService>();
        services.AddScoped<IAdminSessionService, AdminSessionService>();
        services.AddScoped<IUnifiedAuthenticationService, UnifiedAuthenticationService>();
        services.AddScoped<IAdminBootstrapService, AdminBootstrapService>();
        services.AddScoped<IDevelopmentDataSeeder, DevelopmentDataSeeder>();
        services.AddScoped<IDemoDataPurger, DemoDataPurger>();
        services.AddScoped<IAdminCategoryService, AdminCategoryService>();
        services.AddScoped<IPublicCategoryService, PublicCategoryService>();
        services.AddScoped<IAdminProductService, AdminProductService>();
        services.AddScoped<IPublicProductService, PublicProductService>();
        services.AddScoped<IAdminInventoryService, AdminInventoryService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddSingleton<ICustomerPasswordHasher, CustomerPasswordHasher>();
        services.TryAddScoped<IOtpDeliveryService, DisabledOtpDeliveryService>();
        services.AddScoped<ICustomerAuthenticationService, CustomerAuthenticationService>();
        services.AddScoped<ICustomerProfileService, CustomerProfileService>();
        services.AddScoped<IAdminCustomerService, AdminCustomerService>();
        services.AddScoped<ICustomerAddressService, CustomerAddressService>();
        services.AddScoped<IDeliveryLocationService, DeliveryLocationService>();
        services.AddScoped<ICartService, CartService>();
        services.TryAddScoped<IOnlinePaymentGateway, DisabledOnlinePaymentGateway>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<IOrderService, OrderService>();
        services.Configure<FileStorageOptions>(options =>
            options.RootPath = configuration[$"{FileStorageOptions.SectionName}:RootPath"]
                ?? string.Empty);
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IUploadedFileCleanupService>(provider =>
            (IUploadedFileCleanupService)provider.GetRequiredService<IFileService>());
        services.AddScoped<IProductImageService, ProductImageService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<IBannerService, BannerService>();
        services.AddScoped<IContentPageService, ContentPageService>();
        services.AddScoped<IStoreSettingService, StoreSettingService>();
        services.AddScoped<ISocialLinkService, SocialLinkService>();
        services.AddScoped<IContactMessageService, ContactMessageService>();
        services.AddScoped<IHomeService, HomeService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSingleton<INotificationDeliveryProvider>(
            new DisabledNotificationProvider(NotificationChannel.Sms));
        services.AddSingleton<INotificationDeliveryProvider>(
            new DisabledNotificationProvider(NotificationChannel.WhatsApp));
        services.AddSingleton<INotificationDeliveryProvider>(
            new DisabledNotificationProvider(NotificationChannel.Email));
        services.AddHostedService<NotificationOutboxWorker>();
        services.AddScoped<IAuditLogQueryService, AuditLogQueryService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<ISeoService, SeoService>();

        RegisterUseCaseContracts(services);

        return services;
    }

    private static void RegisterUseCaseContracts(IServiceCollection services)
    {
        ServiceDescriptor[] aggregateServices = services
            .Where(descriptor =>
                descriptor.ServiceType.IsInterface
                && descriptor.ImplementationType is not null)
            .ToArray();

        foreach (ServiceDescriptor aggregateService in aggregateServices)
        {
            Type aggregateContract = aggregateService.ServiceType;
            foreach (Type useCaseContract in aggregateContract.GetInterfaces())
            {
                if (services.Any(descriptor => descriptor.ServiceType == useCaseContract))
                {
                    continue;
                }

                services.AddScoped(
                    useCaseContract,
                    provider => provider.GetRequiredService(aggregateContract));
            }
        }
    }
}
