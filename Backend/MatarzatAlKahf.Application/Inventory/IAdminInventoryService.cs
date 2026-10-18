using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Inventory;
public interface IAdminInventoryService : IGetAllAdminInventoryService, IGetLowStockAdminInventoryService, IGetByProductVariantIdAdminInventoryService, IAdjustAdminInventoryService
{
}
