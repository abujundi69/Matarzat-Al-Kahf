using MatarzatAlKahf.Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Customers;

public sealed class OtpVerificationConfiguration : IEntityTypeConfiguration<OtpVerification>
{
    public void Configure(EntityTypeBuilder<OtpVerification> builder)
    {
        builder.ToTable("OtpVerifications", "customers");
        builder.ConfigureBaseEntity();

        builder.Property(verification => verification.PhoneNumber)
            .IsRequired()
            .HasMaxLength(30)
            .IsUnicode();

        builder.Property(verification => verification.CodeHash)
            .IsRequired()
            .HasMaxLength(500)
            .IsUnicode();

        builder.Property(verification => verification.Purpose)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode();

        builder.Property(verification => verification.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.Property(verification => verification.Language)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.Property(verification => verification.AttemptCount)
            .HasDefaultValue(0);
        builder.Property(verification => verification.MaximumAttempts)
            .HasDefaultValue(5);
        builder.Property(verification => verification.VerificationTokenHash)
            .IsRequired()
            .HasMaxLength(128)
            .IsUnicode();

        builder.HasIndex(verification => new
        {
            verification.PhoneNumber,
            verification.Purpose,
            verification.Status,
        });

        builder.HasIndex(verification => verification.ExpiresAtUtc);
    }
}
