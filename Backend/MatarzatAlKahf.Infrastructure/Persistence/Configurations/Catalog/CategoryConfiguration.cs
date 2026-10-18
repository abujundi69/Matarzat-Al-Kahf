using MatarzatAlKahf.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Catalog;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories", "catalog");
        builder.ConfigureBaseEntity();

        builder.Property(category => category.ImageUrl)
            .IsRequired()
            .HasMaxLength(2000)
            .IsUnicode();

        builder.Property(category => category.SortOrder)
            .HasDefaultValue(0);

        builder.Property(category => category.IsActive)
            .HasDefaultValue(false);

        builder.HasOne(category => category.ParentCategory)
            .WithMany(category => category.Children)
            .HasForeignKey(category => category.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(category => category.ParentCategoryId);
        builder.HasIndex(category => new { category.IsActive, category.SortOrder });
    }
}
