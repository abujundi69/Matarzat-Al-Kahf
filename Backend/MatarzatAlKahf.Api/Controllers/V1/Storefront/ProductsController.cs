using MatarzatAlKahf.Application.Catalog.Products;
using MatarzatAlKahf.Application.Common.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Storefront;

[ApiController]
[AllowAnonymous]
[Route("api/v1/products")]
[Produces("application/json", "application/problem+json")]
public sealed class ProductsController(IGetPagePublicProductService getPagePublicProductService, IGetByIdPublicProductService getByIdPublicProductService, IGetBySlugPublicProductService getBySlugPublicProductService, IGetSimilarPublicProductService getSimilarPublicProductService) : ControllerBase
{
    /// <summary>Returns a filtered and paged list of active localized products.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<PublicProductListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<PublicProductListItemDto>>> GetPage([FromQuery] ProductListQuery query, CancellationToken cancellationToken)
    {
        var response = await getPagePublicProductService.GetPageAsync(query, cancellationToken);
        return Ok(response);
    }

    /// <summary>Returns one active product in the requested language.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<PublicProductDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicProductDetailsDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await getByIdPublicProductService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }

    /// <summary>Returns one active localized product by normalized slug.</summary>
    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType<PublicProductDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicProductDetailsDto>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var response = await getBySlugPublicProductService.GetBySlugAsync(slug, cancellationToken);
        return Ok(response);
    }

    /// <summary>Returns available products from the same category.</summary>
    [HttpGet("{id:guid}/similar")]
    public async Task<ActionResult<IReadOnlyList<PublicProductListItemDto>>> GetSimilar(Guid id, [FromQuery] int count = 8, CancellationToken cancellationToken = default)
    {
        var response = await getSimilarPublicProductService.GetSimilarAsync(id, count, cancellationToken);
        return Ok(response);
    }
}
