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
    private static void EnsureCurrency(Cart cart, CurrencyCode currency)
    {
        if (currency != CurrencyCode.ILS)
        {
            throw new ConflictException(
                "Only ILS products can be added to an active cart.");
        }

        CurrencyCode? existing = cart.Items.FirstOrDefault()?.ProductVariant.Product.Currency;
        if (existing.HasValue && existing != currency)
        {
            throw new ConflictException("A cart cannot contain products in different currencies.");
        }
    }
}
