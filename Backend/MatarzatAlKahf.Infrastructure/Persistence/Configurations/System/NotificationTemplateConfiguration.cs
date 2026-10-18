using MatarzatAlKahf.Domain.Entities.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.System;

public sealed class NotificationTemplateConfiguration :
    IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates", "system");
        builder.ConfigureBaseEntity();
        builder.Property(template => template.Code).IsRequired().HasMaxLength(100).IsUnicode();
        builder.Property(template => template.Channel)
            .HasConversion<string>().HasMaxLength(20).IsUnicode();
        builder.Property(template => template.ArabicTitle)
            .IsRequired().HasMaxLength(300).IsUnicode();
        builder.Property(template => template.EnglishTitle)
            .IsRequired().HasMaxLength(300).IsUnicode();
        builder.Property(template => template.ArabicBody)
            .IsRequired().HasMaxLength(4000).IsUnicode();
        builder.Property(template => template.EnglishBody)
            .IsRequired().HasMaxLength(4000).IsUnicode();
        builder.HasIndex(template => new { template.Code, template.Channel }).IsUnique();
    }
}
