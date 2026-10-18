using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.SocialMediaLinks.DTOs;
using MatarzatAlKahf.Application.Features.SocialMediaLinks.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/social-links")]
[Tags("Social Links")]
public sealed class AdminSocialLinksController(IListAdminSocialLinkService listAdminSocialLinkService, IGetSocialLinkService getSocialLinkService, ICreateSocialLinkService createSocialLinkService, IUpdateSocialLinkService updateSocialLinkService, IDeleteSocialLinkService deleteSocialLinkService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.SocialRead)]
    public async Task<ActionResult<IReadOnlyList<SocialLinkResponse>>> List(CancellationToken cancellationToken) => Ok(await listAdminSocialLinkService.ListAdminAsync(cancellationToken));
    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.SocialRead)]
    public async Task<ActionResult<SocialLinkResponse>> Get(Guid id, CancellationToken cancellationToken) => Ok(await getSocialLinkService.GetAsync(id, cancellationToken));
    [HttpPost]
    [Authorize(Policy = PermissionCodes.SocialWrite)]
    public async Task<ActionResult<SocialLinkResponse>> Create(SaveSocialLinkRequest request, CancellationToken cancellationToken)
    {
        SocialLinkResponse response = await createSocialLinkService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.SocialWrite)]
    public async Task<ActionResult<SocialLinkResponse>> Update(Guid id, SaveSocialLinkRequest request, CancellationToken cancellationToken) => Ok(await updateSocialLinkService.UpdateAsync(id, request, cancellationToken));
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionCodes.SocialWrite)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteSocialLinkService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
