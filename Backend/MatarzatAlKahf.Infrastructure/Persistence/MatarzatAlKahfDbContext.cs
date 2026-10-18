using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Entities.Content;
using MatarzatAlKahf.Domain.Entities.Customers;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Entities.Promotions;
using MatarzatAlKahf.Domain.Entities.System;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Persistence;

public sealed class MatarzatAlKahfDbContext(DbContextOptions<MatarzatAlKahfDbContext> options)
    : DbContext(options)
{
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<AdminUserRole> AdminUserRoles => Set<AdminUserRole>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<AdminSession> AdminSessions => Set<AdminSession>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();

    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();

    public DbSet<CustomerSession> CustomerSessions => Set<CustomerSession>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<CategoryTranslation> CategoryTranslations => Set<CategoryTranslation>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductTranslation> ProductTranslations => Set<ProductTranslation>();

    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    public DbSet<ProductImageTranslation> ProductImageTranslations => Set<ProductImageTranslation>();

    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<OrderStatusHistory> OrderStatusHistory => Set<OrderStatusHistory>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<DeliveryLocation> DeliveryLocations => Set<DeliveryLocation>();

    public DbSet<Cart> Carts => Set<Cart>();

    public DbSet<CartItem> CartItems => Set<CartItem>();

    public DbSet<CheckoutIdempotency> CheckoutIdempotencies => Set<CheckoutIdempotency>();

    public DbSet<Coupon> Coupons => Set<Coupon>();

    public DbSet<CouponUsage> CouponUsages => Set<CouponUsage>();

    public DbSet<CouponCategory> CouponCategories => Set<CouponCategory>();

    public DbSet<CouponProduct> CouponProducts => Set<CouponProduct>();

    public DbSet<Banner> Banners => Set<Banner>();

    public DbSet<BannerTranslation> BannerTranslations => Set<BannerTranslation>();

    public DbSet<ContentPage> ContentPages => Set<ContentPage>();

    public DbSet<ContentPageTranslation> ContentPageTranslations => Set<ContentPageTranslation>();

    public DbSet<SocialMediaLink> SocialMediaLinks => Set<SocialMediaLink>();

    public DbSet<StoreSetting> StoreSettings => Set<StoreSetting>();

    public DbSet<StoreSettingTranslation> StoreSettingTranslations => Set<StoreSettingTranslation>();

    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();

    public DbSet<SeoSetting> SeoSettings => Set<SeoSetting>();

    public DbSet<ImportOperation> ImportOperations => Set<ImportOperation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MatarzatAlKahfDbContext).Assembly);
    }
}
