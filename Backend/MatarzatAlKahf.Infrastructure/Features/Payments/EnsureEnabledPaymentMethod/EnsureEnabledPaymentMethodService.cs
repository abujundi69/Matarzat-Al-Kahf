using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Payments.DTOs;
using MatarzatAlKahf.Application.Features.Payments.Interfaces;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Payments.Services;
public sealed partial class PaymentMethodService
{
    public async Task EnsureEnabledAsync(PaymentMethod method, CancellationToken cancellationToken)
    {
        if (method == PaymentMethod.ElectronicPayment)
        {
            throw new ConflictException("Electronic payment is currently unavailable.");
        }

        StoreSetting? setting = await dbContext.StoreSettings.AsNoTracking().SingleOrDefaultAsync(item => item.Key == Key(method) && !item.IsDeleted, cancellationToken);
        bool enabled = setting is null ? DefaultEnabled(method) : ParseEnabled(setting.Value);
        if (!enabled)
        {
            throw new ConflictException($"The {method} payment method is currently disabled.");
        }
    }
}
