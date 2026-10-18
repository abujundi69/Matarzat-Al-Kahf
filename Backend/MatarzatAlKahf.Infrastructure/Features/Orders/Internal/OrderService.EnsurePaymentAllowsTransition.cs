using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Infrastructure.Features.Orders.Services;

public sealed partial class OrderService
{
    private static void EnsurePaymentAllowsTransition(
        Order order,
        OrderStatus newStatus)
    {
        if (newStatus is not (OrderStatus.Confirmed
            or OrderStatus.Preparing
            or OrderStatus.ReadyForShipping
            or OrderStatus.Shipped
            or OrderStatus.Delivered))
        {
            return;
        }

        Payment payment = order.Payments
            .Where(item => !item.IsDeleted)
            .OrderByDescending(item => item.CreatedAtUtc)
            .FirstOrDefault()
            ?? throw new ConflictException(
                "The order cannot progress without a payment record.");
        if (payment.Method != order.SelectedPaymentMethod)
        {
            throw new ConflictException(
                "The order payment method is inconsistent.");
        }

        if (payment.Method == PaymentMethod.CashOnDelivery)
        {
            if (payment.Status is not (PaymentStatus.Pending or PaymentStatus.Paid))
            {
                throw new ConflictException(
                    "The cash-on-delivery payment is not eligible for fulfillment.");
            }

            return;
        }

        if (payment.Status != PaymentStatus.Paid)
        {
            throw new ConflictException(
                "Prepaid orders must have a confirmed payment before fulfillment.");
        }
    }
}
