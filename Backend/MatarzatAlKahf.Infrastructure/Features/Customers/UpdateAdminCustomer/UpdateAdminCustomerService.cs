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
    public async Task<AdminCustomerResponse> UpdateAsync(Guid id, UpdateAdminCustomerRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        Customer customer = await FindAsync(id, cancellationToken);
        customer.FullName = request.FullName.Trim();
        customer.Email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        customer.PreferredLanguage = request.PreferredLanguage;
        customer.AdministrativeNotes = request.AdministrativeNotes?.Trim() ?? string.Empty;
        customer.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("customer.updated", customer, cancellationToken);
        return await GetAsync(id, cancellationToken);
    }
}
