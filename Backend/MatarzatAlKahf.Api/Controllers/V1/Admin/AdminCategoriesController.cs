using MatarzatAlKahf.Application.Catalog.Categories;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;

[ApiController]
[Authorize]
[Route("api/v1/admin/categories")]
[Produces("application/json", "application/problem+json")]
public sealed class AdminCategoriesController(IGetAllAdminCategoryService getAllAdminCategoryService, IGetByIdAdminCategoryService getByIdAdminCategoryService, ICreateAdminCategoryService createAdminCategoryService, IUpdateAdminCategoryService updateAdminCategoryService, IDeleteAdminCategoryService deleteAdminCategoryService) : ControllerBase
{
    /// <summary>Returns administrative category data with Arabic and English translations.</summary>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogCategoriesRead)]
    [ProducesResponseType<PagedResult<AdminCategoryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<AdminCategoryDto>>> GetAll([FromQuery] Guid? parentCategoryId, [FromQuery] int page = PaginationRules.DefaultPage, [FromQuery] int pageSize = PaginationRules.DefaultPageSize, CancellationToken cancellationToken = default)
    {
        var response = await getAllAdminCategoryService.GetAllAsync(parentCategoryId, page, pageSize, cancellationToken);
        return Ok(response);
    }

    /// <summary>Returns one administrative category with both translations.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogCategoriesRead)]
    [ProducesResponseType<AdminCategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminCategoryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await getByIdAdminCategoryService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    /// <summary>Creates a category and its required Arabic and English translations.</summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogCategoriesWrite)]
    [ProducesResponseType<AdminCategoryDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminCategoryDto>> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var response = await createAdminCategoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>Updates a category, hierarchy placement, and both translations.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogCategoriesWrite)]
    [ProducesResponseType<AdminCategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AdminCategoryDto>> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var response = await updateAdminCategoryService.UpdateAsync(id, request, cancellationToken);
        return Ok(response);
    }

    /// <summary>Soft-deletes an empty category while preserving related history.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicyNames.CatalogCategoriesWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteAdminCategoryService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
