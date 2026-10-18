using MatarzatAlKahf.Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Customers;

public sealed class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("CustomerAddresses", "customers");
        builder.ConfigureBaseEntity();

        builder.Property(address => address.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.Property(address => address.City)
            .IsRequired()
            .HasMaxLength(150)
            .IsUnicode();

        builder.Property(address => address.Area)
            .IsRequired()
            .HasMaxLength(150)
            .IsUnicode();

        builder.Property(address => address.AddressLine)
            .IsRequired()
            .HasMaxLength(1000)
            .IsUnicode();

        builder.Property(address => address.TaxNumber)
            .IsRequired()
            .HasMaxLength(100)
            .IsUnicode();

        builder.Property(address => address.DeliveryNotes)
            .IsRequired()
            .HasMaxLength(2000)
            .IsUnicode();

        builder.Property(address => address.IsDefault)
            .HasDefaultValue(false);

        builder.HasOne(address => address.Customer)
            .WithMany(customer => customer.Addresses)
            .HasForeignKey(address => address.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(address => address.DeliveryLocation)
            .WithMany(location => location.Addresses)
            .HasForeignKey(address => address.DeliveryLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(address => new { address.CustomerId, address.IsDefault });
        builder.HasIndex(address => address.DeliveryLocationId);
    }
}
