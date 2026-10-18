using MatarzatAlKahf.Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Customers;

public sealed class CustomerSessionConfiguration : IEntityTypeConfiguration<CustomerSession>
{
    public void Configure(EntityTypeBuilder<CustomerSession> builder)
    {
        builder.ToTable("CustomerSessions", "customers");
        builder.ConfigureBaseEntity();
        builder.Property(session => session.TokenIdHash)
            .IsRequired()
            .HasMaxLength(128)
            .IsUnicode();
        builder.HasOne(session => session.Customer)
            .WithMany(customer => customer.Sessions)
            .HasForeignKey(session => session.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(session => session.TokenIdHash).IsUnique();
        builder.HasIndex(session => new { session.CustomerId, session.RevokedAtUtc });
        builder.HasIndex(session => session.ExpiresAtUtc);
    }
}
