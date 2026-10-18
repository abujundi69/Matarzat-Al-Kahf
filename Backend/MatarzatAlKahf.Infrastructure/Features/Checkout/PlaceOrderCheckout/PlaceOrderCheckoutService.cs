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
    public async Task<PlaceOrderResponse> PlaceOrderAsync(CheckoutRequest request, string idempotencyKey, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateIdempotencyKey(idempotencyKey);
        ValidateRequest(request);

        string normalizedKey = idempotencyKey.Trim();
        string scope = currentActor.CustomerId.HasValue
            ? $"customer:{currentActor.CustomerId.Value:N}"
            : $"guest:{Hash(request.GuestCartToken!)}";
        string requestHash = Hash(JsonSerializer.Serialize(request));
        try
        {
            return await DbTransactionExecutor.ExecuteAsync(
                dbContext,
                token => PlaceCoreAsync(request, normalizedKey, scope, requestHash, token),
                cancellationToken,
                IsolationLevel.Serializable);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException(
                "Inventory or cart data changed while the order was being placed. Review the cart and try again.",
                exception);
        }
        catch (DbUpdateException exception) when (DbUpdateExceptionTranslator.IsUniqueViolation(exception))
        {
            throw new ConflictException(
                "The checkout request conflicts with another order submission. Retry using the same idempotency key.",
                exception);
        }
    }
}
