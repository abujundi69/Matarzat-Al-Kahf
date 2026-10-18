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
    private static void ValidateTemplate(SaveNotificationTemplateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || request.Code.Length > 100 || string.IsNullOrWhiteSpace(request.ArabicTitle) || request.ArabicTitle.Length > 300 || string.IsNullOrWhiteSpace(request.EnglishTitle) || request.EnglishTitle.Length > 300 || string.IsNullOrWhiteSpace(request.ArabicBody) || request.ArabicBody.Length > 4000 || string.IsNullOrWhiteSpace(request.EnglishBody) || request.EnglishBody.Length > 4000)
        {
            throw ValidationException.For(nameof(request), "Notification template data is invalid.");
        }
    }
}
