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
    private async Task<OtpVerification> GetVerifiedAsync(string phone, string token, OtpPurpose purpose, CancellationToken cancellationToken)
    {
        string hash = HashToken(token);
        OtpVerification? verification = await dbContext.OtpVerifications.SingleOrDefaultAsync(otp => otp.PhoneNumber == phone && otp.Purpose == purpose && otp.Status == OtpStatus.Verified && otp.VerificationTokenHash == hash && otp.ConsumedAtUtc == null, cancellationToken);
        if (verification is null || verification.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw ValidationException.For(nameof(token), "Verification token is invalid or expired.");
        }

        return verification;
    }
}
