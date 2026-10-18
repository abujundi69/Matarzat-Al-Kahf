using System.Security.Cryptography;
using System.Text;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Coupons.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Coupons.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Coupons.Interfaces;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Entities.Promotions;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Coupons.Services;
public sealed partial class CouponService
{
    private static void ValidateRequest(SaveCouponRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(request.Code) || request.Code.Trim().Length > 100)
        {
            errors[nameof(request.Code)] = ["Code is required and cannot exceed 100 characters."];
        }

        if (request.DiscountValue <= 0 || (request.DiscountType == DiscountType.Percentage && request.DiscountValue > 100))
        {
            errors[nameof(request.DiscountValue)] = ["Discount value is invalid."];
        }

        if (request.MinimumOrderAmount < 0 || request.MaximumDiscountAmount < 0)
        {
            errors[nameof(request.MinimumOrderAmount)] = ["Amounts cannot be negative."];
        }

        if (request.StartsAtUtc.HasValue && request.EndsAtUtc.HasValue && request.StartsAtUtc >= request.EndsAtUtc)
        {
            errors[nameof(request.EndsAtUtc)] = ["End date must follow start date."];
        }

        if (request.TotalUsageLimit <= 0 || request.UsageLimitPerCustomer <= 0)
        {
            errors[nameof(request.TotalUsageLimit)] = ["Usage limits must be positive."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Coupon data is invalid.", errors);
        }
    }
}
