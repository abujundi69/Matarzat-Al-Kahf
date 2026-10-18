using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;

namespace MatarzatAlKahf.Application.Features.Notifications.Interfaces;
public interface IDeleteTemplateNotificationService
{
    Task DeleteTemplateAsync(Guid id, CancellationToken cancellationToken);
}
