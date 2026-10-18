using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.ContentPages.DTOs;
using MatarzatAlKahf.Application.Features.ContentPages.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/pages")]
[Tags("Content Pages")]
public sealed class AdminContentPagesController(IListAdminContentPageService listAdminContentPageService, IGetContentPageService getContentPageService, ICreateContentPageService createContentPageService, IUpdateContentPageService updateContentPageService, IDeleteContentPageService deleteContentPageService, ISetPublishedContentPageService setPublishedContentPageService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.PagesRead)]
    public async Task<ActionResult<IReadOnlyList<AdminContentPageResponse>>> List(CancellationToken cancellationToken) => Ok(await listAdminContentPageService.ListAdminAsync(cancellationToken));
    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.PagesRead)]
    public async Task<ActionResult<AdminContentPageResponse>> Get(Guid id, CancellationToken cancellationToken) => Ok(await getContentPageService.GetAsync(id, cancellationToken));
    [HttpPost]
    [Authorize(Policy = PermissionCodes.PagesWrite)]
    public async Task<ActionResult<AdminContentPageResponse>> Create(SaveContentPageRequest request, CancellationToken cancellationToken)
    {
        AdminContentPageResponse response = await createContentPageService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.PagesWrite)]
    public async Task<ActionResult<AdminContentPageResponse>> Update(Guid id, SaveContentPageRequest request, CancellationToken cancellationToken) => Ok(await updateContentPageService.UpdateAsync(id, request, cancellationToken));
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionCodes.PagesWrite)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteContentPageService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Policy = PermissionCodes.PagesWrite)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
    {
        await setPublishedContentPageService.SetPublishedAsync(id, true, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/unpublish")]
    [Authorize(Policy = PermissionCodes.PagesWrite)]
    public async Task<IActionResult> Unpublish(Guid id, CancellationToken cancellationToken)
    {
        await setPublishedContentPageService.SetPublishedAsync(id, false, cancellationToken);
        return NoContent();
    }
}
