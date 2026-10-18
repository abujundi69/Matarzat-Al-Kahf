using MatarzatAlKahf.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Orders;

public sealed class CheckoutIdempotencyConfiguration :
    IEntityTypeConfiguration<CheckoutIdempotency>
{
    public void Configure(EntityTypeBuilder<CheckoutIdempotency> builder)
    {
        builder.ToTable("CheckoutIdempotencies", "sales");
        builder.ConfigureBaseEntity();
        builder.Property(record => record.Scope).IsRequired().HasMaxLength(100).IsUnicode();
        builder.Property(record => record.Key).IsRequired().HasMaxLength(200).IsUnicode();
        builder.Property(record => record.RequestHash).IsRequired().HasMaxLength(128).IsUnicode();
        builder.HasOne(record => record.Order).WithMany()
            .HasForeignKey(record => record.OrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(record => new { record.Scope, record.Key }).IsUnique();
        builder.HasIndex(record => record.OrderId);
        builder.HasIndex(record => record.ExpiresAtUtc);
    }
}
