using MatarzatAlKahf.Application.Features.ContentPages.DTOs;
using MatarzatAlKahf.Application.Features.ContentPages.Interfaces;
using MatarzatAlKahf.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Storefront;
[ApiController]
[Route("api/v1/pages")]
[Tags("Content Pages")]
public sealed class ContentPagesController(IListPublicContentPageService listPublicContentPageService, IGetPublicBySlugContentPageService getPublicBySlugContentPageService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ContentPageResponse>>> List(ContentPageType? type, CancellationToken cancellationToken) => Ok(await listPublicContentPageService.ListPublicAsync(type, cancellationToken));
    [HttpGet("{slug}")]
    public async Task<ActionResult<ContentPageResponse>> Get(string slug, CancellationToken cancellationToken) => Ok(await getPublicBySlugContentPageService.GetPublicBySlugAsync(slug, cancellationToken));
}
