using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Validation;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Inventory;
public interface IInventoryAdjustmentValidator
{
    void Validate(InventoryAdjustmentRequest request, int currentQuantityOnHand);
}
