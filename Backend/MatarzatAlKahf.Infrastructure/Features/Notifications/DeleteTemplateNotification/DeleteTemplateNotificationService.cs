using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;
using MatarzatAlKahf.Application.Features.Notifications.Interfaces;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Notifications.Services;
public sealed partial class NotificationService
{
    public async Task DeleteTemplateAsync(Guid id, CancellationToken cancellationToken)
    {
        NotificationTemplate template = await dbContext.NotificationTemplates.SingleOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken) ?? throw new NotFoundException("Notification template was not found.");
        template.IsDeleted = true;
        template.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
