using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.Dashboard.DTOs;
public sealed record DashboardNotificationResponse(Guid Id, NotificationChannel Channel, NotificationStatus Status, string Title, DateTime CreatedAtUtc);
