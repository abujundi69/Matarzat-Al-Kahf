using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;

namespace MatarzatAlKahf.Application.Features.Notifications.Interfaces;
public interface ISaveTemplateNotificationService
{
    Task<NotificationTemplateResponse> SaveTemplateAsync(Guid? id, SaveNotificationTemplateRequest request, CancellationToken cancellationToken);
}
