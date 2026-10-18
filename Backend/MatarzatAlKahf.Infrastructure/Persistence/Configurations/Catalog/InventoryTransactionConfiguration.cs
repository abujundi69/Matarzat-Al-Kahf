using MatarzatAlKahf.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MatarzatAlKahf.Infrastructure.Persistence.Configurations.Catalog;

public sealed class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions", "catalog");
        builder.ConfigureBaseEntity();

        builder.Property(transaction => transaction.Type)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsUnicode();

        builder.Property(transaction => transaction.Reference)
            .HasMaxLength(250)
            .IsUnicode();

        builder.Property(transaction => transaction.Notes)
            .HasMaxLength(2000)
            .IsUnicode();

        builder.HasOne(transaction => transaction.InventoryItem)
            .WithMany(inventory => inventory.Transactions)
            .HasForeignKey(transaction => transaction.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(transaction => transaction.InventoryItemId);
        builder.HasIndex(transaction => transaction.Reference)
            .HasFilter("[Reference] IS NOT NULL");
    }
}
