using System.Net.Mail;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.ContactMessages.DTOs;
using MatarzatAlKahf.Application.Features.ContactMessages.Interfaces;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;
using MatarzatAlKahf.Domain.Entities.Content;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Content.Services;
public sealed partial class ContactMessageService
{
    public async Task<ContactMessageResponse> ReplyAsync(Guid id, ReplyContactMessageRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reply) || request.Reply.Length > 10000)
        {
            throw ValidationException.For(nameof(request.Reply), "Reply is required and cannot exceed 10000 characters.");
        }

        ContactMessage message = await FindAsync(id, true, cancellationToken);
        message.AdministrativeReply = request.Reply.Trim();
        message.Status = ContactMessageStatus.Replied;
        message.RepliedAtUtc = DateTime.UtcNow;
        message.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        if (message.CustomerId.HasValue)
        {
            bool arabic = message.Language == LanguageCode.Arabic;
            await notificationService.EnqueueAsync(new EnqueueNotificationRequest(message.CustomerId, null, null, message.Language, NotificationChannel.InApp, arabic ? "تم الرد على رسالتك" : "Your message received a reply", arabic ? $"تم الرد على رسالتك بخصوص \"{message.Subject}\"." : $"Your message about \"{message.Subject}\" received a reply.", message.PhoneNumber), cancellationToken);
        }

        return Map(message);
    }
}
