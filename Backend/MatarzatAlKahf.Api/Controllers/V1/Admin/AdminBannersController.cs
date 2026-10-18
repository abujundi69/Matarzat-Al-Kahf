using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Banners.DTOs;
using MatarzatAlKahf.Application.Features.Banners.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/banners")]
[Tags("Banners")]
public sealed class AdminBannersController(IListAdminBannerService listAdminBannerService, IGetBannerService getBannerService, ICreateBannerService createBannerService, IUpdateBannerService updateBannerService, IDeleteBannerService deleteBannerService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.BannersRead)]
    public async Task<ActionResult<IReadOnlyList<AdminBannerResponse>>> List(CancellationToken cancellationToken) => Ok(await listAdminBannerService.ListAdminAsync(cancellationToken));
    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.BannersRead)]
    public async Task<ActionResult<AdminBannerResponse>> Get(Guid id, CancellationToken cancellationToken) => Ok(await getBannerService.GetAsync(id, cancellationToken));
    [HttpPost]
    [Authorize(Policy = PermissionCodes.BannersWrite)]
    public async Task<ActionResult<AdminBannerResponse>> Create(SaveBannerRequest request, CancellationToken cancellationToken)
    {
        AdminBannerResponse response = await createBannerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.BannersWrite)]
    public async Task<ActionResult<AdminBannerResponse>> Update(Guid id, SaveBannerRequest request, CancellationToken cancellationToken) => Ok(await updateBannerService.UpdateAsync(id, request, cancellationToken));
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionCodes.BannersWrite)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteBannerService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
