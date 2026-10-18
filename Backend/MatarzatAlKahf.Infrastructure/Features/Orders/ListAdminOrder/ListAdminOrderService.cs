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
    public Task<PagedResult<OrderSummaryResponse>> ListAdminAsync(OrderQuery query, CancellationToken cancellationToken)
    {
        PaginationRules.Validate(query.Page, query.PageSize);
        if (query.FromUtc.HasValue && query.ToUtc.HasValue && query.FromUtc > query.ToUtc)
        {
            throw ValidationException.For(nameof(query.ToUtc), "End date must follow start date.");
        }

        IQueryable<Order> orders = dbContext.Orders.Where(order => !order.IsDeleted);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search = query.Search.Trim();
            orders = orders.Where(order => order.OrderNumber.Contains(search) || order.CustomerName.Contains(search) || order.PhoneNumber.Contains(search));
        }

        if (query.Status.HasValue)
        {
            orders = orders.Where(order => order.Status == query.Status);
        }

        if (query.PaymentMethod.HasValue)
        {
            orders = orders.Where(order => order.SelectedPaymentMethod == query.PaymentMethod);
        }

        if (query.PaymentStatus.HasValue)
        {
            PaymentStatus paymentStatus = query.PaymentStatus.Value;
            orders = orders.Where(order => order.Payments
                .Where(payment => !payment.IsDeleted)
                .OrderByDescending(payment => payment.CreatedAtUtc)
                .Select(payment => (PaymentStatus?)payment.Status)
                .FirstOrDefault() == paymentStatus);
        }

        if (query.FromUtc.HasValue)
        {
            orders = orders.Where(order => order.CreatedAtUtc >= query.FromUtc);
        }

        if (query.ToUtc.HasValue)
        {
            orders = orders.Where(order => order.CreatedAtUtc <= query.ToUtc);
        }

        return PageAsync(orders, query.Page, query.PageSize, cancellationToken);
    }
}
