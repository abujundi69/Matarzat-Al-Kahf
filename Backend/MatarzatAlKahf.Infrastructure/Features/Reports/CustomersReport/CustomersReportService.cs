using System.Globalization;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Reports.DTOs;
using MatarzatAlKahf.Application.Features.Reports.Interfaces;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Reports.Services;
public sealed partial class ReportService
{
    public async Task<CustomerReportResponse> CustomersAsync(ReportQuery query, CancellationToken cancellationToken)
    {
        Validate(query);
        IQueryable<Domain.Entities.Customers.Customer> customers = dbContext.Customers.Where(customer => !customer.IsDeleted);
        int count = await customers.CountAsync(cancellationToken);
        DateTime inactiveBefore = DateTime.UtcNow.AddDays(-90);
        int newCount = await customers.CountAsync(customer => customer.CreatedAtUtc >= query.FromUtc && customer.CreatedAtUtc < query.ToUtc, cancellationToken);
        int inactive = await customers.CountAsync(customer => !customer.LastLoginAtUtc.HasValue || customer.LastLoginAtUtc < inactiveBefore, cancellationToken);
        var customerRows = await customers.AsNoTracking().Select(customer => new { customer.Id, customer.FullName, customer.PhoneNumber, OrderCount = customer.Orders.Count(order => !order.IsDeleted && order.CreatedAtUtc >= query.FromUtc && order.CreatedAtUtc < query.ToUtc), Spending = customer.Orders.Where(order => !order.IsDeleted && order.Status == OrderStatus.Delivered && order.CreatedAtUtc >= query.FromUtc && order.CreatedAtUtc < query.ToUtc).Sum(order => (decimal? )order.TotalAmount) ?? 0, customer.CreatedAtUtc, customer.LastLoginAtUtc }).OrderByDescending(item => item.Spending).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(cancellationToken);
        List<CustomerReportItem> items = customerRows.Select(item => new CustomerReportItem(item.Id, item.FullName, item.PhoneNumber, item.OrderCount, item.Spending, item.CreatedAtUtc, item.LastLoginAtUtc)).ToList();
        return new CustomerReportResponse(newCount, inactive, PagedResultFactory.Create(items, query.Page, query.PageSize, count));
    }
}
