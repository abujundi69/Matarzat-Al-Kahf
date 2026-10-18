using MatarzatAlKahf.Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Customers;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers", "customers");
        builder.ConfigureBaseEntity();

        builder.Property(customer => customer.FullName)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode();

        builder.Property(customer => customer.PhoneNumber)
            .IsRequired()
            .HasMaxLength(16)
            .IsUnicode();

        builder.Property(customer => customer.NormalizedPhoneNumber)
            .IsRequired()
            .HasMaxLength(16)
            .IsUnicode(false);

        builder.Property(customer => customer.Email)
            .IsRequired()
            .HasMaxLength(320)
            .IsUnicode();

        builder.Property(customer => customer.PasswordHash)
            .IsRequired()
            .HasMaxLength(500)
            .IsUnicode();

        builder.Property(customer => customer.AdministrativeNotes)
            .IsRequired()
            .HasMaxLength(4000)
            .IsUnicode();

        builder.Property(customer => customer.IsPhoneVerified)
            .HasDefaultValue(false);

        builder.Property(customer => customer.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.Property(customer => customer.PreferredLanguage)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.HasIndex(customer => customer.NormalizedPhoneNumber)
            .IsUnique();

        builder.HasIndex(customer => customer.Email);
    }
}
