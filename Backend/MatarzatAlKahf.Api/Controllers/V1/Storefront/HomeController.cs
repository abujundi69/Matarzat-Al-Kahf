using MatarzatAlKahf.Application.Features.Home.DTOs;
using MatarzatAlKahf.Application.Features.Home.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Storefront;

[ApiController]
[Route("api/v1/home")]
[Tags("Home")]
public sealed class HomeController(IHomeService homeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HomeResponse>> Get(CancellationToken cancellationToken) =>
        Ok(await homeService.GetAsync(cancellationToken));
}
