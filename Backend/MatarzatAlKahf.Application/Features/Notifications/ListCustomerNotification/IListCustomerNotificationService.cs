using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;

namespace MatarzatAlKahf.Application.Features.Notifications.Interfaces;
public interface IListCustomerNotificationService
{
    Task<PagedResult<NotificationResponse>> ListCustomerAsync(int page, int pageSize, CancellationToken cancellationToken);
}
