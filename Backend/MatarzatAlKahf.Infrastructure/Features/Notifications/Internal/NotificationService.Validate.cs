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
    private static void Validate(EnqueueNotificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 250 || string.IsNullOrWhiteSpace(request.Body) || request.Body.Length > 4000 || request.Recipient.Length > 320)
        {
            throw ValidationException.For(nameof(request), "Notification data is invalid.");
        }
    }
}
