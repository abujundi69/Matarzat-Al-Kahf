using System.Security.Cryptography;
using System.Text;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Carts.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Carts.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Carts.Interfaces;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Entities.Promotions;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Carts.Services;
public sealed partial class CartService
{
    private static decimal CalculateDiscount(Coupon coupon, decimal subtotal)
    {
        decimal value = coupon.DiscountType == DiscountType.Percentage ? subtotal * coupon.DiscountValue / 100 : coupon.DiscountValue;
        if (coupon.MaximumDiscountAmount.HasValue)
        {
            value = Math.Min(value, coupon.MaximumDiscountAmount.Value);
        }

        return Math.Min(subtotal, decimal.Round(value, 2));
    }
}
