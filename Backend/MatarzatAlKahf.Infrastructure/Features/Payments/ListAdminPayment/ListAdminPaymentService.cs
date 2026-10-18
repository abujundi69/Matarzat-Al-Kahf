using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Files.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Files.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Files.Interfaces;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;
using MatarzatAlKahf.Application.Features.Notifications.Interfaces;
using MatarzatAlKahf.Application.Features.Payments.DTOs;
using MatarzatAlKahf.Application.Features.Payments.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Payments.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Payments.Interfaces;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Payments.Services;
public sealed partial class PaymentService
{
    public async Task<PagedResult<PaymentResponse>> ListAdminAsync(PaymentQuery query, CancellationToken cancellationToken)
    {
        PaginationRules.Validate(query.Page, query.PageSize);
        IQueryable<Payment> payments = dbContext.Payments.AsNoTracking().Where(payment => !payment.IsDeleted);
        if (query.Status.HasValue)
        {
            payments = payments.Where(payment => payment.Status == query.Status);
        }

        if (query.Method.HasValue)
        {
            payments = payments.Where(payment => payment.Method == query.Method);
        }

        if (query.FromUtc.HasValue)
        {
            payments = payments.Where(payment => payment.CreatedAtUtc >= query.FromUtc);
        }

        if (query.ToUtc.HasValue)
        {
            payments = payments.Where(payment => payment.CreatedAtUtc <= query.ToUtc);
        }

        int count = await payments.CountAsync(cancellationToken);
        List<Payment> rows = await payments.OrderByDescending(payment => payment.CreatedAtUtc).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(cancellationToken);
        return PagedResultFactory.Create(rows.Select(payment => Map(payment, true)).ToArray(), query.Page, query.PageSize, count);
    }
}
