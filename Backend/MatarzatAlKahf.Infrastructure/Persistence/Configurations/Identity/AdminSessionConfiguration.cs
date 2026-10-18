using MatarzatAlKahf.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Identity;

public sealed class AdminSessionConfiguration : IEntityTypeConfiguration<AdminSession>
{
    public void Configure(EntityTypeBuilder<AdminSession> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("AdminSessions", "identity");
        builder.ConfigureBaseEntity();
        builder.Property(session => session.TokenIdHash)
            .IsRequired()
            .HasMaxLength(128)
            .IsUnicode();
        builder.HasOne(session => session.AdminUser)
            .WithMany(user => user.Sessions)
            .HasForeignKey(session => session.AdminUserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(session => session.TokenIdHash).IsUnique();
        builder.HasIndex(session => new { session.AdminUserId, session.RevokedAtUtc });
        builder.HasIndex(session => session.ExpiresAtUtc);
    }
}
