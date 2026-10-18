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
    private static async Task<PagedResult<OrderSummaryResponse>> PageAsync(IQueryable<Order> query, int page, int pageSize, CancellationToken cancellationToken)
    {
        PaginationRules.Validate(page, pageSize);
        int count = await query.CountAsync(cancellationToken);
        List<OrderSummaryResponse> items = await query.AsNoTracking().OrderByDescending(order => order.CreatedAtUtc).ThenByDescending(order => order.Id).Skip((page - 1) * pageSize).Take(pageSize).Select(order => new OrderSummaryResponse(order.Id, order.OrderNumber, order.CustomerName, order.PhoneNumber, order.Status, order.SelectedPaymentMethod, order.Payments.Where(payment => !payment.IsDeleted).OrderByDescending(payment => payment.CreatedAtUtc).Select(payment => payment.Status).FirstOrDefault(), order.TotalAmount, order.Currency, order.CreatedAtUtc)).ToListAsync(cancellationToken);
        return PagedResultFactory.Create(items, page, pageSize, count);
    }
}
