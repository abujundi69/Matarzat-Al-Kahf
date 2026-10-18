using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;

namespace MatarzatAlKahf.Application.Features.Notifications.Interfaces;
public interface IMarkReadNotificationService
{
    Task MarkReadAsync(Guid id, CancellationToken cancellationToken);
}
