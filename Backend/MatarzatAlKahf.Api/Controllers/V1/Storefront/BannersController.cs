using MatarzatAlKahf.Application.Features.Banners.DTOs;
using MatarzatAlKahf.Application.Features.Banners.Interfaces;
using MatarzatAlKahf.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Storefront;
[ApiController]
[Route("api/v1/banners")]
[Tags("Banners")]
public sealed class BannersController(IListPublicBannerService listPublicBannerService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BannerResponse>>> List(BannerPosition? position, CancellationToken cancellationToken) => Ok(await listPublicBannerService.ListPublicAsync(position, cancellationToken));
}
