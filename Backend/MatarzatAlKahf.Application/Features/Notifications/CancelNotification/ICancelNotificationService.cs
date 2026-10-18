using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;

namespace MatarzatAlKahf.Application.Features.Notifications.Interfaces;
public interface ICancelNotificationService
{
    Task CancelAsync(Guid id, CancellationToken cancellationToken);
}
