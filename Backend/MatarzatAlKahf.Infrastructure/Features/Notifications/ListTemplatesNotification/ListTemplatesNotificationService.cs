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
    public async Task<IReadOnlyList<NotificationTemplateResponse>> ListTemplatesAsync(CancellationToken cancellationToken) => await dbContext.NotificationTemplates.AsNoTracking().Where(template => !template.IsDeleted).OrderBy(template => template.Code).ThenBy(template => template.Channel).Select(template => MapTemplate(template)).ToListAsync(cancellationToken);
}
