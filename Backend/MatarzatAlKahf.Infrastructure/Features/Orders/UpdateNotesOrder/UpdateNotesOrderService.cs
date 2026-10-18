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
    public async Task<OrderDetailsResponse> UpdateNotesAsync(Guid id, UpdateOrderNotesRequest request, CancellationToken cancellationToken)
    {
        if (request.Notes.Length > 4000)
        {
            throw ValidationException.For(nameof(request.Notes), "Notes cannot exceed 4000 characters.");
        }

        Order order = await FindAdminAsync(id, cancellationToken);
        order.InternalNotes = request.Notes.Trim();
        order.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.RecordAsync("order.notes.updated", nameof(Order), order.Id, $"Internal notes for order {order.OrderNumber} were updated.", cancellationToken);
        return Map(order, true);
    }
}
