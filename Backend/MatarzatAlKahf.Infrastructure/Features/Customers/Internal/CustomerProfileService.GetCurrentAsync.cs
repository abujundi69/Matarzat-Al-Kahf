using System.Net.Mail;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
using MatarzatAlKahf.Application.Features.Customers.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Customers.Interfaces;
using MatarzatAlKahf.Domain.Entities.Customers;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Customers.Services;
public sealed partial class CustomerProfileService
{
    private async Task<Customer> GetCurrentAsync(CancellationToken cancellationToken)
    {
        Guid id = currentActor.CustomerId ?? throw new AuthenticationFailedException("Customer authentication is required.");
        return await dbContext.Customers.SingleOrDefaultAsync(customer => customer.Id == id && !customer.IsDeleted && customer.Status == Domain.Enums.UserStatus.Active, cancellationToken) ?? throw new AuthenticationFailedException("Customer session is invalid.");
    }
}
