using MatarzatAlKahf.Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Content;

public sealed class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.ToTable("ContactMessages", "content");
        builder.ConfigureBaseEntity();

        builder.Property(message => message.Name)
            .IsRequired()
            .HasMaxLength(200)
            .IsUnicode();

        builder.Property(message => message.PhoneNumber)
            .IsRequired()
            .HasMaxLength(30)
            .IsUnicode();

        builder.Property(message => message.Email)
            .IsRequired()
            .HasMaxLength(320)
            .IsUnicode();

        builder.Property(message => message.Subject)
            .IsRequired()
            .HasMaxLength(250)
            .IsUnicode();

        builder.Property(message => message.Message)
            .IsRequired()
            .HasMaxLength(10000)
            .IsUnicode();

        builder.Property(message => message.AdministrativeReply)
            .IsRequired()
            .HasMaxLength(10000)
            .IsUnicode();

        builder.Property(message => message.Language)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.Property(message => message.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsUnicode();

        builder.HasOne(message => message.Customer)
            .WithMany(customer => customer.ContactMessages)
            .HasForeignKey(message => message.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(message => new { message.Status, message.CreatedAtUtc });
        builder.HasIndex(message => message.Email);
        builder.HasIndex(message => message.CustomerId);
    }
}
