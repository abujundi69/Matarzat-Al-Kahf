using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;

namespace MatarzatAlKahf.Application.Features.Notifications.Interfaces;
public interface IListTemplatesNotificationService
{
    Task<IReadOnlyList<NotificationTemplateResponse>> ListTemplatesAsync(CancellationToken cancellationToken);
}
