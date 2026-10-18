using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Notifications.DTOs;
public sealed record NotificationQuery(int Page = 1, int PageSize = 20, NotificationStatus? Status = null, NotificationChannel? Channel = null);
