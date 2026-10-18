using MatarzatAlKahf.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Orders;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", "sales");
        builder.ConfigureBaseEntity();

        builder.Property(payment => payment.Method)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode();

        builder.Property(payment => payment.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode();

        builder.Property(payment => payment.Amount).HasPrecision(18, 2);

        builder.Property(payment => payment.Currency)
            .HasConversion<string>()
            .HasMaxLength(3)
            .IsUnicode();

        builder.Property(payment => payment.ProviderName)
            .IsRequired()
            .HasMaxLength(150)
            .IsUnicode();

        builder.Property(payment => payment.TransactionReference)
            .IsRequired()
            .HasMaxLength(250)
            .IsUnicode();

        builder.Property(payment => payment.ProviderResponseReference)
            .IsRequired()
            .HasMaxLength(250)
            .IsUnicode();

        builder.Property(payment => payment.ProofFilePath)
            .IsRequired().HasMaxLength(1000).IsUnicode();
        builder.Property(payment => payment.VerificationNote)
            .IsRequired().HasMaxLength(2000).IsUnicode();

        builder.HasOne(payment => payment.Order)
            .WithMany(order => order.Payments)
            .HasForeignKey(payment => payment.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(payment => payment.VerifiedByAdminUser)
            .WithMany()
            .HasForeignKey(payment => payment.VerifiedByAdminUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(payment => payment.OrderId);
        builder.HasIndex(payment => payment.TransactionReference);
        builder.HasIndex(payment => new { payment.Status, payment.CreatedAtUtc });
        builder.HasIndex(payment => payment.VerifiedByAdminUserId);
    }
}
