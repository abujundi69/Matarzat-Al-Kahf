using MatarzatAlKahf.Application.Features.SocialMediaLinks.DTOs;
using MatarzatAlKahf.Application.Features.SocialMediaLinks.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Storefront;
[ApiController]
[Route("api/v1/social-links")]
[Tags("Social Links")]
public sealed class SocialLinksController(IListPublicSocialLinkService listPublicSocialLinkService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SocialLinkResponse>>> List(CancellationToken cancellationToken) => Ok(await listPublicSocialLinkService.ListPublicAsync(cancellationToken));
}
