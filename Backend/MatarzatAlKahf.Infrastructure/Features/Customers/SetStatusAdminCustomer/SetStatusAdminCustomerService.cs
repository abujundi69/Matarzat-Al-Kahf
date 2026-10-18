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
    public async Task SetStatusAsync(Guid id, UserStatus status, CancellationToken cancellationToken)
    {
        if (status is not (UserStatus.Active or UserStatus.Suspended or UserStatus.Disabled))
        {
            throw ValidationException.For(nameof(status), "Unsupported customer status.");
        }

        Customer customer = await FindAsync(id, cancellationToken);
        customer.Status = status;
        customer.UpdatedAtUtc = DateTime.UtcNow;
        if (status != UserStatus.Active)
        {
            await dbContext.CustomerSessions.Where(session => session.CustomerId == id && session.RevokedAtUtc == null).ExecuteUpdateAsync(updates => updates.SetProperty(session => session.RevokedAtUtc, DateTime.UtcNow), cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync($"customer.status.{status.ToString().ToLowerInvariant()}", customer, cancellationToken);
    }
}
