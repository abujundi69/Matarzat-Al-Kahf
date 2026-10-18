using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;

namespace MatarzatAlKahf.Application.Features.Notifications.Interfaces;
public interface IListAdminNotificationService
{
    Task<PagedResult<NotificationResponse>> ListAdminAsync(NotificationQuery query, CancellationToken cancellationToken);
}
