using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;
using MatarzatAlKahf.Application.Features.Notifications.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/notifications")]
[Tags("Notifications")]
public sealed class AdminNotificationsController(IListAdminNotificationService listAdminNotificationService, IRetryNotificationService retryNotificationService, ICancelNotificationService cancelNotificationService, IListTemplatesNotificationService listTemplatesNotificationService, ISaveTemplateNotificationService saveTemplateNotificationService, IDeleteTemplateNotificationService deleteTemplateNotificationService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PermissionCodes.NotificationsRead)]
    public async Task<ActionResult<PagedResult<NotificationResponse>>> List([FromQuery] NotificationQuery query, CancellationToken cancellationToken) => Ok(await listAdminNotificationService.ListAdminAsync(query, cancellationToken));
    [HttpPost("{id:guid}/retry")]
    [Authorize(Policy = PermissionCodes.NotificationsWrite)]
    public async Task<IActionResult> Retry(Guid id, CancellationToken cancellationToken)
    {
        await retryNotificationService.RetryAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = PermissionCodes.NotificationsWrite)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await cancelNotificationService.CancelAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("templates")]
    [Authorize(Policy = PermissionCodes.NotificationsRead)]
    public async Task<ActionResult<IReadOnlyList<NotificationTemplateResponse>>> Templates(CancellationToken cancellationToken) => Ok(await listTemplatesNotificationService.ListTemplatesAsync(cancellationToken));
    [HttpPost("templates")]
    [Authorize(Policy = PermissionCodes.NotificationsWrite)]
    public async Task<ActionResult<NotificationTemplateResponse>> CreateTemplate(SaveNotificationTemplateRequest request, CancellationToken cancellationToken)
    {
        NotificationTemplateResponse result = await saveTemplateNotificationService.SaveTemplateAsync(null, request, cancellationToken);
        return CreatedAtAction(nameof(Templates), new { }, result);
    }

    [HttpPut("templates/{id:guid}")]
    [Authorize(Policy = PermissionCodes.NotificationsWrite)]
    public async Task<ActionResult<NotificationTemplateResponse>> UpdateTemplate(Guid id, SaveNotificationTemplateRequest request, CancellationToken cancellationToken) => Ok(await saveTemplateNotificationService.SaveTemplateAsync(id, request, cancellationToken));
    [HttpDelete("templates/{id:guid}")]
    [Authorize(Policy = PermissionCodes.NotificationsWrite)]
    public async Task<IActionResult> DeleteTemplate(Guid id, CancellationToken cancellationToken)
    {
        await deleteTemplateNotificationService.DeleteTemplateAsync(id, cancellationToken);
        return NoContent();
    }
}
