using MatarzatAlKahf.Application.Catalog.Products;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;

[ApiController]
[Authorize]
[Route("api/v1/admin/products")]
[Produces("application/json", "application/problem+json")]
public sealed class AdminProductsController(IGetAllAdminProductService getAllAdminProductService, IGetByIdAdminProductService getByIdAdminProductService, ICreateAdminProductService createAdminProductService, IUpdateAdminProductService updateAdminProductService, IDeleteAdminProductService deleteAdminProductService) : ControllerBase
{
    /// <summary>Returns products with both translations, images, variants, and inventory.</summary>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogProductsRead)]
    [ProducesResponseType<PagedResult<AdminProductDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<AdminProductDto>>> GetAll([FromQuery] int page = PaginationRules.DefaultPage, [FromQuery] int pageSize = PaginationRules.DefaultPageSize, CancellationToken cancellationToken = default)
    {
        var response = await getAllAdminProductService.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(response);
    }

    /// <summary>Returns one product with its complete administrative catalog data.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogProductsRead)]
    [ProducesResponseType<AdminProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await getByIdAdminProductService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    /// <summary>Creates a product, translations, variants, inventory, and image metadata.</summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogProductsWrite)]
    [ProducesResponseType<AdminProductDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminProductDto>> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var response = await createAdminProductService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>Updates product data and child metadata without overwriting existing stock.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogProductsWrite)]
    [ProducesResponseType<AdminProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminProductDto>> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var response = await updateAdminProductService.UpdateAsync(id, request, cancellationToken);
        return Ok(response);
    }

    /// <summary>Marks a product and its variants unavailable while preserving history.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogProductsWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteAdminProductService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
