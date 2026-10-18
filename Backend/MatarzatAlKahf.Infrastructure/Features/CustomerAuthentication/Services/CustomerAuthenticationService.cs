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
public sealed partial class CustomerAuthenticationService(MatarzatAlKahfDbContext dbContext, IPhoneNumberNormalizer phoneNormalizer, IPasswordPolicy passwordPolicy, ICustomerPasswordHasher passwordHasher, ICustomerTokenService tokenService, IOtpDeliveryService otpDeliveryService, INotificationService notificationService) : ICustomerAuthenticationService
{
    private const int OtpLifetimeMinutes = 5;
    private const int ResendCooldownSeconds = 60;
    private const int MaximumAttempts = 5;
    private const int MaximumRequestsPerHour = 5;
    private readonly PasswordHasher<OtpVerification> _otpHasher = new();
}
