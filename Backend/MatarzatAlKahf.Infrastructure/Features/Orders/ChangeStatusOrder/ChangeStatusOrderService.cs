using System.Data;
using System.Net;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;
using MatarzatAlKahf.Application.Features.Notifications.Interfaces;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Orders.Interfaces;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Orders.Services;
public sealed partial class OrderService
{
    public async Task<OrderDetailsResponse> ChangeStatusAsync(Guid id, ChangeOrderStatusRequest request, CancellationToken cancellationToken)
    {
        return await DbTransactionExecutor.ExecuteAsync(dbContext, async token =>
        {
            Order order = await FindAdminAsync(id, token);
            if (!AllowedTransitions.TryGetValue(order.Status, out OrderStatus[]? allowed) || !allowed.Contains(request.Status))
            {
                throw new ConflictException($"Order cannot transition from {order.Status} to {request.Status}.");
            }

            EnsurePaymentAllowsTransition(order, request.Status);
            await ApplyTransitionAsync(order, request.Status, request.Note, currentActor.AdminUserId, token);
            await auditService.RecordAsync("order.status.changed", nameof(Order), order.Id, $"Order {order.OrderNumber} changed to {request.Status}.", token);
            if (order.CustomerId.HasValue)
            {
                await notificationService.EnqueueAsync(new EnqueueNotificationRequest(order.CustomerId, null, order.Id, order.Language, NotificationChannel.InApp, order.Language == LanguageCode.Arabic ? "تحديث حالة الطلب" : "Order status updated", order.Language == LanguageCode.Arabic ? $"حالة الطلب {order.OrderNumber}: {request.Status}" : $"Order {order.OrderNumber}: {request.Status}", order.PhoneNumber), token);
            }

            return Map(order, true);
        }, cancellationToken, IsolationLevel.Serializable);
    }
}
