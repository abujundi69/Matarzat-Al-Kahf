using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;
using MatarzatAlKahf.Application.Features.Notifications.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Customer;
[ApiController]
[Route("api/v1/customer/notifications")]
[Authorize(Policy = AuthenticationPolicyNames.Customer)]
[Tags("Notifications")]
public sealed class CustomerNotificationsController(IListCustomerNotificationService listCustomerNotificationService, IMarkReadNotificationService markReadNotificationService, IMarkAllReadNotificationService markAllReadNotificationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<NotificationResponse>>> List(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default) => Ok(await listCustomerNotificationService.ListCustomerAsync(page, pageSize, cancellationToken));
    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> Read(Guid id, CancellationToken cancellationToken)
    {
        await markReadNotificationService.MarkReadAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> ReadAll(CancellationToken cancellationToken)
    {
        await markAllReadNotificationService.MarkAllReadAsync(cancellationToken);
        return NoContent();
    }
}
