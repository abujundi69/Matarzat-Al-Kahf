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
    public async Task<NotificationTemplateResponse> SaveTemplateAsync(Guid? id, SaveNotificationTemplateRequest request, CancellationToken cancellationToken)
    {
        ValidateTemplate(request);
        NotificationTemplate template;
        if (id.HasValue)
        {
            template = await dbContext.NotificationTemplates.SingleOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken) ?? throw new NotFoundException("Notification template was not found.");
        }
        else
        {
            template = new NotificationTemplate();
            dbContext.NotificationTemplates.Add(template);
        }

        string code = request.Code.Trim().ToLowerInvariant();
        if (await dbContext.NotificationTemplates.AnyAsync(item => item.Id != template.Id && item.Code == code && item.Channel == request.Channel && !item.IsDeleted, cancellationToken))
        {
            throw new ConflictException("A template already exists for this code and channel.");
        }

        template.Code = code;
        template.Channel = request.Channel;
        template.ArabicTitle = request.ArabicTitle.Trim();
        template.EnglishTitle = request.EnglishTitle.Trim();
        template.ArabicBody = request.ArabicBody.Trim();
        template.EnglishBody = request.EnglishBody.Trim();
        template.IsActive = request.IsActive;
        template.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapTemplate(template);
    }
}
