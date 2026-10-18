using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Validation;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Inventory;
public static class InventoryRules
{
    public static void EnsureQuantityCannotBecomeNegative(int currentQuantityOnHand, int quantityChange)
    {
        if ((long)currentQuantityOnHand + quantityChange < 0)
        {
            throw new ConflictException("Inventory quantity cannot become negative.");
        }
    }

    public static bool DirectionMatches(InventoryTransactionType type, int quantityChange)
    {
        if (quantityChange == 0)
        {
            return false;
        }

        return type switch
        {
            InventoryTransactionType.AdjustmentIncrease => quantityChange > 0,
            InventoryTransactionType.AdjustmentDecrease => quantityChange < 0,
            _ => false
        };
    }

    public static bool IsLowStock(int quantityOnHand, int reservedQuantity, int reorderLevel)
    {
        return quantityOnHand - reservedQuantity <= reorderLevel;
    }

    public static bool IsValidRowVersion(string? rowVersion)
    {
        if (string.IsNullOrWhiteSpace(rowVersion))
        {
            return false;
        }

        try
        {
            return Convert.FromBase64String(rowVersion).Length > 0;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
