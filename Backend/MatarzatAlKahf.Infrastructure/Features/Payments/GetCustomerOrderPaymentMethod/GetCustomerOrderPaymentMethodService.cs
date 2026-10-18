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
    public async Task<PaymentMethodResponse> GetCustomerOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        Guid customerId = currentActor.CustomerId ?? throw new AuthenticationFailedException("Customer authentication is required.");
        PaymentMethod? method = await dbContext.Orders.AsNoTracking().Where(order => order.Id == orderId && order.CustomerId == customerId && !order.IsDeleted).Select(order => (PaymentMethod? )order.SelectedPaymentMethod).SingleOrDefaultAsync(cancellationToken);
        if (!method.HasValue)
        {
            throw new NotFoundException("Order was not found.");
        }

        IReadOnlyList<PaymentMethodResponse> methods = await LoadAsync(cancellationToken);
        return methods.Single(item => item.Method == method.Value);
    }
}
