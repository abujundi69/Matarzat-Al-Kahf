using MatarzatAlKahf.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Catalog;

public sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems", "catalog");
        builder.ConfigureBaseEntity();

        builder.Property(inventory => inventory.QuantityOnHand)
            .HasDefaultValue(0);

        builder.Property(inventory => inventory.ReservedQuantity)
            .HasDefaultValue(0);

        builder.Property(inventory => inventory.ReorderLevel)
            .HasDefaultValue(0);

        builder.Property(inventory => inventory.RowVersion)
            .IsRowVersion();

        builder.HasOne(inventory => inventory.ProductVariant)
            .WithOne(variant => variant.InventoryItem)
            .HasForeignKey<InventoryItem>(inventory => inventory.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(inventory => inventory.ProductVariantId)
            .IsUnique();
    }
}
