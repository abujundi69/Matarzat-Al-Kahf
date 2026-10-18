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
    private async Task<OtpRequestResponse> RequestOtpAsync(RequestOtpRequest request, OtpPurpose purpose, bool hideAccountExistence, CancellationToken cancellationToken)
    {
        string phone = phoneNormalizer.Normalize(request.PhoneNumber);
        DateTime now = DateTime.UtcNow;
        OtpVerification? latest = await dbContext.OtpVerifications.Where(otp => otp.PhoneNumber == phone && otp.Purpose == purpose).OrderByDescending(otp => otp.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);
        if (latest is not null && latest.LastSentAtUtc.AddSeconds(ResendCooldownSeconds) > now)
        {
            throw new RateLimitExceededException("Please wait before requesting another OTP.");
        }

        int recentCount = await dbContext.OtpVerifications.CountAsync(otp => otp.PhoneNumber == phone && otp.Purpose == purpose && otp.CreatedAtUtc >= now.AddHours(-1), cancellationToken);
        if (recentCount >= MaximumRequestsPerHour)
        {
            throw new RateLimitExceededException("Too many OTP requests. Try again later.");
        }

        if (!hideAccountExistence && await dbContext.Customers.AnyAsync(customer => customer.NormalizedPhoneNumber == phone && !customer.IsDeleted, cancellationToken))
        {
            throw new ConflictException("A customer with this phone number already exists.");
        }

        string code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", CultureInfo.InvariantCulture);
        var verification = new OtpVerification
        {
            PhoneNumber = phone,
            Purpose = purpose,
            Status = OtpStatus.Pending,
            Language = request.Language,
            ExpiresAtUtc = now.AddMinutes(OtpLifetimeMinutes),
            LastSentAtUtc = now,
            MaximumAttempts = MaximumAttempts,
            VerificationTokenHash = string.Empty
        };
        verification.CodeHash = _otpHasher.HashPassword(verification, code);
        dbContext.OtpVerifications.Add(verification);
        await dbContext.SaveChangesAsync(cancellationToken);
        try
        {
            await otpDeliveryService.DeliverAsync(phone, code, purpose, request.Language, cancellationToken);
        }
        catch
        {
            verification.Status = OtpStatus.Cancelled;
            await dbContext.SaveChangesAsync(CancellationToken.None);
            throw;
        }

        return new OtpRequestResponse(verification.ExpiresAtUtc, ResendCooldownSeconds);
    }
}
