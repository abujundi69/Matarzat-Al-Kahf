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
    private async Task<OtpVerificationResponse> VerifyOtpAsync(VerifyOtpRequest request, OtpPurpose purpose, CancellationToken cancellationToken)
    {
        string phone = phoneNormalizer.Normalize(request.PhoneNumber);
        OtpVerification verification = await dbContext.OtpVerifications.Where(otp => otp.PhoneNumber == phone && otp.Purpose == purpose && otp.Status == OtpStatus.Pending).OrderByDescending(otp => otp.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken) ?? throw ValidationException.For(nameof(request.Code), "OTP is invalid or expired.");
        DateTime now = DateTime.UtcNow;
        if (verification.ExpiresAtUtc <= now || verification.AttemptCount >= verification.MaximumAttempts)
        {
            verification.Status = OtpStatus.Expired;
            await dbContext.SaveChangesAsync(cancellationToken);
            throw ValidationException.For(nameof(request.Code), "OTP is invalid or expired.");
        }

        verification.AttemptCount++;
        bool valid = _otpHasher.VerifyHashedPassword(verification, verification.CodeHash, request.Code) != PasswordVerificationResult.Failed;
        if (!valid)
        {
            if (verification.AttemptCount >= verification.MaximumAttempts)
            {
                verification.Status = OtpStatus.Expired;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            throw ValidationException.For(nameof(request.Code), "OTP is invalid or expired.");
        }

        string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        verification.Status = OtpStatus.Verified;
        verification.VerifiedAtUtc = now;
        verification.VerificationTokenHash = HashToken(token);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new OtpVerificationResponse(token, verification.ExpiresAtUtc);
    }
}
