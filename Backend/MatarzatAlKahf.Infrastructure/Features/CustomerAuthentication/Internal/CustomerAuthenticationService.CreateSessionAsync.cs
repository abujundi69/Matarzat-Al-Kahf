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
    private async Task<CustomerLoginResponse> CreateSessionAsync(Customer customer, CancellationToken cancellationToken)
    {
        CustomerAccessToken token = tokenService.CreateAccessToken(new CustomerTokenDescriptor(customer.Id, customer.FullName, customer.PhoneNumber, customer.PreferredLanguage));
        dbContext.CustomerSessions.Add(new CustomerSession { CustomerId = customer.Id, TokenIdHash = HashToken(token.TokenId), ExpiresAtUtc = token.ExpiresAtUtc });
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CustomerLoginResponse(token.Value, "Bearer", token.ExpiresAtUtc, new CustomerProfileResponse(customer.Id, customer.FullName, customer.PhoneNumber, customer.Email, customer.PreferredLanguage));
    }
}
