using MatarzatAlKahf.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Orders;

public sealed class DeliveryLocationConfiguration : IEntityTypeConfiguration<DeliveryLocation>
{
    public void Configure(EntityTypeBuilder<DeliveryLocation> builder)
    {
        builder.ToTable("DeliveryLocations", "sales");
        builder.ConfigureBaseEntity();
        builder.Property(location => location.CountryCode).IsRequired().HasMaxLength(2).IsUnicode();
        builder.Property(location => location.CountryNameArabic).IsRequired().HasMaxLength(150).IsUnicode();
        builder.Property(location => location.CountryNameEnglish).IsRequired().HasMaxLength(150).IsUnicode();
        builder.Property(location => location.NameArabic).IsRequired().HasMaxLength(150).IsUnicode();
        builder.Property(location => location.NameEnglish).IsRequired().HasMaxLength(150).IsUnicode();
        builder.Property(location => location.AdditionalDeliveryCost).HasPrecision(18, 2);
        builder.Property(location => location.MinimumOrderAmount).HasPrecision(18, 2);
        builder.HasIndex(location => new
        {
            location.CountryCode,
            location.NameEnglish,
            location.IsDeleted
        }).IsUnique();
        builder.HasIndex(location => new { location.IsActive, location.SortOrder });
    }
}
