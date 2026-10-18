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
    public async Task<CustomerLoginResponse> CompleteRegistrationAsync(CompleteRegistrationRequest request, CancellationToken cancellationToken)
    {
        ValidateProfile(request.FullName, request.Email);
        passwordPolicy.Validate(request.Password);
        string phone = phoneNormalizer.Normalize(request.PhoneNumber);
        OtpVerification verification = await GetVerifiedAsync(phone, request.VerificationToken, OtpPurpose.Registration, cancellationToken);
        if (await dbContext.Customers.AnyAsync(customer => customer.NormalizedPhoneNumber == phone && !customer.IsDeleted, cancellationToken))
        {
            throw new ConflictException("A customer with this phone number already exists.");
        }

        var customer = new Customer
        {
            FullName = request.FullName.Trim(),
            PhoneNumber = phone,
            NormalizedPhoneNumber = phone,
            Email = NormalizeEmail(request.Email),
            IsPhoneVerified = true,
            Status = UserStatus.Active,
            PreferredLanguage = request.PreferredLanguage
        };
        customer.PasswordHash = passwordHasher.HashPassword(customer, request.Password);
        verification.Status = OtpStatus.Cancelled;
        verification.ConsumedAtUtc = DateTime.UtcNow;
        dbContext.Customers.Add(customer);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)when (DbUpdateExceptionTranslator.IsUniqueViolation(exception))
        {
            throw new ConflictException("A customer with this phone number already exists.", exception);
        }

        CustomerLoginResponse response = await CreateSessionAsync(customer, cancellationToken);
        await notificationService.EnqueueAsync(new EnqueueNotificationRequest(customer.Id, null, null, customer.PreferredLanguage, NotificationChannel.InApp, "Welcome", "Your customer account has been created.", customer.PhoneNumber), cancellationToken);
        await notificationService.EnqueueAsync(new EnqueueNotificationRequest(null, null, null, LanguageCode.Arabic, NotificationChannel.InApp, "New customer", $"Customer {customer.FullName} registered.", "administrators"), cancellationToken);
        return response;
    }
}
