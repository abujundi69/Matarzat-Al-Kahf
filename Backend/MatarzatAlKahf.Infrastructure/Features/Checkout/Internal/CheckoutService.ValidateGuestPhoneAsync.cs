using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Checkout.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Checkout.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Checkout.Interfaces;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;
using MatarzatAlKahf.Application.Features.Notifications.Interfaces;
using MatarzatAlKahf.Application.Features.Payments.Interfaces;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Entities.Customers;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Entities.Promotions;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Checkout.Services;
public sealed partial class CheckoutService
{
    private async Task ValidateGuestPhoneAsync(string phone, string? verificationToken, bool consume, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(verificationToken))
        {
            throw ValidationException.For(nameof(verificationToken), "Guest phone verification is required.");
        }

        string hash = Hash(verificationToken);
        OtpVerification? verification = await dbContext.OtpVerifications.SingleOrDefaultAsync(otp => otp.PhoneNumber == phone && otp.Purpose == OtpPurpose.ConfirmOrder && otp.Status == OtpStatus.Verified && otp.VerificationTokenHash == hash && otp.ConsumedAtUtc == null, cancellationToken);
        if (verification is null || verification.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw ValidationException.For(nameof(verificationToken), "Guest phone verification is invalid or expired.");
        }

        if (consume)
        {
            verification.Status = OtpStatus.Cancelled;
            verification.ConsumedAtUtc = DateTime.UtcNow;
        }
    }
}
