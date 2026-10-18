using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;

[ApiController]
[Authorize]
[Route("api/v1/admin/inventory")]
[Produces("application/json", "application/problem+json")]
public sealed class AdminInventoryController(IGetAllAdminInventoryService getAllAdminInventoryService, IGetLowStockAdminInventoryService getLowStockAdminInventoryService, IGetByProductVariantIdAdminInventoryService getByProductVariantIdAdminInventoryService, IAdjustAdminInventoryService adjustAdminInventoryService) : ControllerBase
{
    /// <summary>Returns current inventory state for all catalog variants.</summary>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogInventoryRead)]
    [ProducesResponseType<PagedResult<AdminInventoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<AdminInventoryDto>>> GetAll([FromQuery] int page = PaginationRules.DefaultPage, [FromQuery] int pageSize = PaginationRules.DefaultPageSize, CancellationToken cancellationToken = default)
    {
        var response = await getAllAdminInventoryService.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(response);
    }

    /// <summary>Returns variants whose available quantity is at or below their reorder level.</summary>
    [HttpGet("low-stock")]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogInventoryRead)]
    [ProducesResponseType<PagedResult<AdminInventoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<AdminInventoryDto>>> GetLowStock([FromQuery] int page = PaginationRules.DefaultPage, [FromQuery] int pageSize = PaginationRules.DefaultPageSize, CancellationToken cancellationToken = default)
    {
        var response = await getLowStockAdminInventoryService.GetLowStockAsync(page, pageSize, cancellationToken);
        return Ok(response);
    }

    /// <summary>Returns current inventory state for one product variant.</summary>
    [HttpGet("{productVariantId:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogInventoryRead)]
    [ProducesResponseType<AdminInventoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminInventoryDto>> GetByProductVariantId(Guid productVariantId, CancellationToken cancellationToken)
    {
        var response = await getByProductVariantIdAdminInventoryService.GetByProductVariantIdAsync(productVariantId, cancellationToken);
        return Ok(response);
    }

    /// <summary>Adjusts stock using a signed change and an optimistic-concurrency token.</summary>
    [HttpPost("{productVariantId:guid}/adjust")]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogInventoryWrite)]
    [ProducesResponseType<InventoryAdjustmentResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InventoryAdjustmentResultDto>> Adjust(Guid productVariantId, [FromBody] InventoryAdjustmentRequest request, CancellationToken cancellationToken)
    {
        var response = await adjustAdminInventoryService.AdjustAsync(productVariantId, request, cancellationToken);
        return Ok(response);
    }
}
