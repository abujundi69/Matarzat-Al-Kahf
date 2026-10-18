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
    private async Task ApplyTransitionAsync(Order order, OrderStatus newStatus, string? note, Guid? adminUserId, CancellationToken cancellationToken)
    {
        OrderStatus previous = order.Status;
        if (newStatus == OrderStatus.Cancelled)
        {
            CancelPaymentForOrderCancellation(order);
            ReleaseReservation(order, false);
        }
        else if (newStatus == OrderStatus.Delivered)
        {
            CommitReservation(order);
            SettleCashOnDelivery(order, adminUserId);
        }
        else if (newStatus == OrderStatus.Returned)
        {
            if (previous == OrderStatus.Delivered)
            {
                ReleaseReservation(order, true);
            }
            else
            {
                ReleaseReservation(order, false);
            }
        }

        order.Status = newStatus;
        order.UpdatedAtUtc = DateTime.UtcNow;
        var history = new OrderStatusHistory
        {
            PreviousStatus = previous,
            NewStatus = newStatus,
            ChangedByAdminUserId = adminUserId,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim()
        };
        order.StatusHistory.Add(history);
        dbContext.Set<OrderStatusHistory>().Add(history);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException("Inventory changed while updating the order.", exception);
        }
    }
}
