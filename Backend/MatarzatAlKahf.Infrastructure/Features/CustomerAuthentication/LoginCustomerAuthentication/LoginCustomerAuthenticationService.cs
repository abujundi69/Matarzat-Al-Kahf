using System.Globalization;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Requests;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Responses;
using MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;
using MatarzatAlKahf.Application.Features.Notifications.Interfaces;
using MatarzatAlKahf.Domain.Entities.Customers;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.CustomerAuthentication.Services;
public sealed partial class CustomerAuthenticationService
{
    public async Task<CustomerLoginResponse> LoginAsync(CustomerLoginRequest request, CancellationToken cancellationToken)
    {
        string phone = phoneNormalizer.Normalize(request.PhoneNumber);
        Customer? customer = await dbContext.Customers.SingleOrDefaultAsync(candidate => candidate.NormalizedPhoneNumber == phone && !candidate.IsDeleted, cancellationToken);
        bool valid = customer is not null && passwordHasher.VerifyPassword(customer, request.Password);
        if (!valid)
        {
            throw new AuthenticationFailedException("Invalid phone number or password.");
        }

        if (customer!.Status != UserStatus.Active || !customer.IsPhoneVerified)
        {
            throw new AuthorizationDeniedException("This account is not active and verified.");
        }

        customer.LastLoginAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await CreateSessionAsync(customer, cancellationToken);
    }
}
