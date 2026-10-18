using MatarzatAlKahf.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Orders;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", "sales");
        builder.ConfigureBaseEntity();

        builder.Property(order => order.OrderNumber)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode();

        builder.Property(order => order.Language)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.Property(order => order.CustomerName)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode();

        builder.Property(order => order.PhoneNumber)
            .IsRequired()
            .HasMaxLength(30)
            .IsUnicode();

        builder.Property(order => order.City)
            .IsRequired()
            .HasMaxLength(150)
            .IsUnicode();

        builder.Property(order => order.DeliveryAreaNameArabic)
            .IsRequired()
            .HasMaxLength(150)
            .IsUnicode();

        builder.Property(order => order.DeliveryAreaNameEnglish)
            .IsRequired()
            .HasMaxLength(150)
            .IsUnicode();

        builder.Property(order => order.Area)
            .IsRequired()
            .HasMaxLength(150)
            .IsUnicode();

        builder.Property(order => order.AddressLine)
            .IsRequired()
            .HasMaxLength(1000)
            .IsUnicode();

        builder.Property(order => order.TaxNumber)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode();

        builder.Property(order => order.DeliveryNotes)
            .IsRequired()
            .HasMaxLength(2000)
            .IsUnicode();

        builder.Property(order => order.InternalNotes)
            .IsRequired()
            .HasMaxLength(4000)
            .IsUnicode();

        builder.Property(order => order.Currency)
            .HasConversion<string>()
            .HasMaxLength(3)
            .IsUnicode();

        builder.Property(order => order.Subtotal).HasPrecision(18, 2);
        builder.Property(order => order.DiscountAmount).HasPrecision(18, 2);
        builder.Property(order => order.DeliveryAmount).HasPrecision(18, 2);
        builder.Property(order => order.TotalAmount).HasPrecision(18, 2);

        builder.Property(order => order.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode();

        builder.Property(order => order.SelectedPaymentMethod)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode();

        builder.HasOne(order => order.Customer)
            .WithMany(customer => customer.Orders)
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(order => order.Coupon)
            .WithMany(coupon => coupon.Orders)
            .HasForeignKey(order => order.CouponId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(order => order.DeliveryLocation)
            .WithMany()
            .HasForeignKey(order => order.DeliveryLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(order => order.OrderNumber)
            .IsUnique();

        builder.HasIndex(order => order.CustomerId);
        builder.HasIndex(order => order.CouponId);
        builder.HasIndex(order => order.DeliveryLocationId);
        builder.HasIndex(order => new { order.Status, order.CreatedAtUtc });
    }
}
