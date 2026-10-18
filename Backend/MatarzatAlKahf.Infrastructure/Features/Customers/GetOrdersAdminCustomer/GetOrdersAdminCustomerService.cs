using System.Linq.Expressions;
using System.Net.Mail;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.CustomerAddresses.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Customers.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Customers.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Customers.Interfaces;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
using MatarzatAlKahf.Domain.Entities.Customers;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Customers.Services;
public sealed partial class AdminCustomerService
{
    public async Task<PagedResult<OrderSummaryResponse>> GetOrdersAsync(Guid id, int page, int pageSize, CancellationToken cancellationToken)
    {
        PaginationRules.Validate(page, pageSize);
        _ = await GetAsync(id, cancellationToken);
        IQueryable<Domain.Entities.Orders.Order> orders = dbContext.Orders.Where(order => order.CustomerId == id && !order.IsDeleted);
        int count = await orders.CountAsync(cancellationToken);
        List<OrderSummaryResponse> items = await orders.AsNoTracking().OrderByDescending(order => order.CreatedAtUtc).Skip((page - 1) * pageSize).Take(pageSize).Select(order => new OrderSummaryResponse(order.Id, order.OrderNumber, order.CustomerName, order.PhoneNumber, order.Status, order.SelectedPaymentMethod, order.Payments.OrderByDescending(payment => payment.CreatedAtUtc).Select(payment => payment.Status).FirstOrDefault(), order.TotalAmount, order.Currency, order.CreatedAtUtc)).ToListAsync(cancellationToken);
        return PagedResultFactory.Create(items, page, pageSize, count);
    }
}
