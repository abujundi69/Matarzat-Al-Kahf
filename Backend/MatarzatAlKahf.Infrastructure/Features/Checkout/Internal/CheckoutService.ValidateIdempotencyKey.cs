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
    private static void ValidateIdempotencyKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Trim().Length is < 8 or > 200)
        {
            throw ValidationException.For(nameof(key), "Idempotency-Key must contain between 8 and 200 characters.");
        }
    }
}
