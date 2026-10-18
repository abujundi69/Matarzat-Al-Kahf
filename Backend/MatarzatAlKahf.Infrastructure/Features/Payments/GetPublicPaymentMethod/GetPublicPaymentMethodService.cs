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
    public async Task<IReadOnlyList<PaymentMethodResponse>> GetPublicAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<PaymentMethodResponse> methods = await LoadAsync(cancellationToken);
        return methods.Where(method => method.IsEnabled || method.Method == PaymentMethod.ElectronicPayment).ToArray();
    }
}
