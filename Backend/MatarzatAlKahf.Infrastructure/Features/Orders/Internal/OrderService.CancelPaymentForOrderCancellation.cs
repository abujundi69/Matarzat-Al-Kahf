using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Infrastructure.Features.Orders.Services;

public sealed partial class OrderService
{
    private static void CancelPaymentForOrderCancellation(Order order)
    {
        Payment? payment = order.Payments
            .Where(item => !item.IsDeleted)
            .OrderByDescending(item => item.CreatedAtUtc)
            .FirstOrDefault();
        if (payment is null
            || payment.Status is PaymentStatus.Cancelled
                or PaymentStatus.Failed
                or PaymentStatus.Refunded)
        {
            return;
        }

        if (payment.Status is PaymentStatus.Paid or PaymentStatus.PartiallyRefunded)
        {
            throw new ConflictException(
                "Paid orders must be fully refunded before cancellation.");
        }

        if (payment.Status == PaymentStatus.Processing
            && payment.Method == PaymentMethod.CreditCard)
        {
            throw new ConflictException(
                "Online payment must be cancelled through the configured provider before cancelling the order.");
        }

        if (payment.Status is PaymentStatus.Pending or PaymentStatus.Processing)
        {
            payment.Status = PaymentStatus.Cancelled;
            payment.VerificationNote = "Payment cancelled with order.";
            payment.UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
