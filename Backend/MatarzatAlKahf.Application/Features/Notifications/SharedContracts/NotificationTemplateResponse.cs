using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Notifications.DTOs;
public sealed record NotificationTemplateResponse(Guid Id, string Code, NotificationChannel Channel, string ArabicTitle, string EnglishTitle, string ArabicBody, string EnglishBody, bool IsActive);
