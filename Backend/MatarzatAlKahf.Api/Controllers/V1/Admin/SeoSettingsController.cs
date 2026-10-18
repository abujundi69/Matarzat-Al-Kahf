using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Seo.DTOs;
using MatarzatAlKahf.Application.Features.Seo.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/seo")]
[Tags("SEO")]
public sealed class SeoSettingsController(IGetSeoService getSeoService, IUpdateSeoService updateSeoService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.SeoRead)]
    public async Task<ActionResult<SeoSettingsResponse>> Get(CancellationToken cancellationToken) => Ok(await getSeoService.GetAsync(cancellationToken));
    [HttpPut]
    [Authorize(Policy = PermissionCodes.SeoWrite)]
    public async Task<ActionResult<SeoSettingsResponse>> Update(SaveSeoSettingsRequest request, CancellationToken cancellationToken) => Ok(await updateSeoService.UpdateAsync(request, cancellationToken));
}
