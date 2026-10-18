using MatarzatAlKahf.Application.Catalog.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Storefront;

[ApiController]
[AllowAnonymous]
[Route("api/v1/categories")]
[Produces("application/json", "application/problem+json")]
public sealed class CategoriesController(IGetAllPublicCategoryService getAllPublicCategoryService, IGetByIdPublicCategoryService getByIdPublicCategoryService, IGetBySlugPublicCategoryService getBySlugPublicCategoryService) : ControllerBase
{
    /// <summary>Returns active categories in the requested language.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PublicCategoryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PublicCategoryDto>>> GetAll([FromQuery] Guid? parentCategoryId, CancellationToken cancellationToken)
    {
        var response = await getAllPublicCategoryService.GetAllAsync(parentCategoryId, cancellationToken);
        return Ok(response);
    }

    /// <summary>Returns one active localized category by identifier.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<PublicCategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicCategoryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await getByIdPublicCategoryService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    /// <summary>Returns one active localized category by normalized slug.</summary>
    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType<PublicCategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicCategoryDto>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var response = await getBySlugPublicCategoryService.GetBySlugAsync(slug, cancellationToken);
        return Ok(response);
    }
}
