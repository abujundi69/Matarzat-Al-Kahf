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
    public async Task ResetPasswordAsync(ResetCustomerPasswordRequest request, CancellationToken cancellationToken)
    {
        passwordPolicy.Validate(request.NewPassword);
        string phone = phoneNormalizer.Normalize(request.PhoneNumber);
        OtpVerification verification = await GetVerifiedAsync(phone, request.VerificationToken, OtpPurpose.ResetPassword, cancellationToken);
        Customer? customer = await dbContext.Customers.SingleOrDefaultAsync(candidate => candidate.NormalizedPhoneNumber == phone && !candidate.IsDeleted && candidate.Status == UserStatus.Active, cancellationToken);
        if (customer is null)
        {
            throw new ValidationException("Password reset could not be completed.", new Dictionary<string, string[]>());
        }

        customer.PasswordHash = passwordHasher.HashPassword(customer, request.NewPassword);
        customer.UpdatedAtUtc = DateTime.UtcNow;
        verification.Status = OtpStatus.Cancelled;
        verification.ConsumedAtUtc = DateTime.UtcNow;
        await dbContext.CustomerSessions.Where(session => session.CustomerId == customer.Id && session.RevokedAtUtc == null).ExecuteUpdateAsync(updates => updates.SetProperty(session => session.RevokedAtUtc, DateTime.UtcNow), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
