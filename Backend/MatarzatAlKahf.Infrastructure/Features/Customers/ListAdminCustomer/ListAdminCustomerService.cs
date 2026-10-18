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
    public async Task<PagedResult<AdminCustomerResponse>> ListAsync(AdminCustomerQuery query, CancellationToken cancellationToken)
    {
        PaginationRules.Validate(query.Page, query.PageSize);
        IQueryable<Customer> customers = dbContext.Customers.Where(customer => !customer.IsDeleted);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search = query.Search.Trim();
            customers = customers.Where(customer => customer.FullName.Contains(search) || customer.PhoneNumber.Contains(search) || customer.Email.Contains(search));
        }

        if (query.Status.HasValue)
        {
            customers = customers.Where(customer => customer.Status == query.Status);
        }

        if (query.RegisteredFromUtc.HasValue)
        {
            customers = customers.Where(customer => customer.CreatedAtUtc >= query.RegisteredFromUtc);
        }

        if (query.RegisteredToUtc.HasValue)
        {
            customers = customers.Where(customer => customer.CreatedAtUtc <= query.RegisteredToUtc);
        }

        int count = await customers.CountAsync(cancellationToken);
        List<AdminCustomerResponse> items = await customers.AsNoTracking().OrderByDescending(customer => customer.CreatedAtUtc).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).Select(CustomerProjection).ToListAsync(cancellationToken);
        return PagedResultFactory.Create(items, query.Page, query.PageSize, count);
    }
}
