using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Validation;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Inventory;
public sealed class InventoryAdjustmentValidator : IInventoryAdjustmentValidator
{
    public void Validate(InventoryAdjustmentRequest request, int currentQuantityOnHand)
    {
        ArgumentNullException.ThrowIfNull(request);
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        ValidationRuleUtilities.AddIf(errors, request.QuantityChange == 0, nameof(request.QuantityChange), "Quantity change cannot be zero.");
        ValidationRuleUtilities.AddIf(errors, !Enum.IsDefined(request.Type), nameof(request.Type), "Inventory transaction type is not supported.");
        ValidationRuleUtilities.AddIf(errors, !InventoryRules.DirectionMatches(request.Type, request.QuantityChange), nameof(request.Type), "Inventory transaction type does not match the quantity direction.");
        ValidationRuleUtilities.AddIf(errors, request.Reference?.Length > 250, nameof(request.Reference), "Reference cannot exceed 250 characters.");
        ValidationRuleUtilities.AddIf(errors, request.Notes?.Length > 2000, nameof(request.Notes), "Notes cannot exceed 2000 characters.");
        ValidationRuleUtilities.AddIf(errors, !InventoryRules.IsValidRowVersion(request.RowVersion), nameof(request.RowVersion), "Row version must be a non-empty base64 value.");
        ValidationRuleUtilities.ThrowIfAny(errors, "Inventory adjustment is invalid.");
        InventoryRules.EnsureQuantityCannotBecomeNegative(currentQuantityOnHand, request.QuantityChange);
    }
}
