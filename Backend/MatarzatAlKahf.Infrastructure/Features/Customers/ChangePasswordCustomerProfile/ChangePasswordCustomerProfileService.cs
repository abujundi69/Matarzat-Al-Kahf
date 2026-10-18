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
    public async Task ChangePasswordAsync(ChangeCustomerPasswordRequest request, CancellationToken cancellationToken)
    {
        Customer customer = await GetCurrentAsync(cancellationToken);
        if (!passwordHasher.VerifyPassword(customer, request.CurrentPassword))
        {
            throw new AuthenticationFailedException("Current password is invalid.");
        }

        passwordPolicy.Validate(request.NewPassword);
        customer.PasswordHash = passwordHasher.HashPassword(customer, request.NewPassword);
        customer.UpdatedAtUtc = DateTime.UtcNow;
        // Other devices must lose access once the password changes; the caller's own session
        // is kept so the request that performed the change does not fail afterwards.
        string currentHash = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(currentActor.SessionTokenId)));
        DateTime revokedAtUtc = DateTime.UtcNow;
        await dbContext.CustomerSessions
            .Where(session => session.CustomerId == customer.Id
                && session.RevokedAtUtc == null
                && session.TokenIdHash != currentHash)
            .ExecuteUpdateAsync(
                updates => updates.SetProperty(session => session.RevokedAtUtc, revokedAtUtc),
                cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
