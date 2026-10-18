using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Infrastructure.Features.Orders.Services;

public sealed partial class OrderService
{
    private static void SettleCashOnDelivery(
        Order order,
        Guid? adminUserId)
    {
        if (order.SelectedPaymentMethod != PaymentMethod.CashOnDelivery)
        {
            return;
        }

        Payment payment = order.Payments
            .Where(item => !item.IsDeleted)
            .OrderByDescending(item => item.CreatedAtUtc)
            .FirstOrDefault()
            ?? throw new ConflictException(
                "The cash-on-delivery payment record is unavailable.");
        if (payment.Status == PaymentStatus.Paid)
        {
            return;
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            throw new ConflictException(
                "The cash-on-delivery payment cannot be settled in its current state.");
        }

        DateTime now = DateTime.UtcNow;
        payment.Status = PaymentStatus.Paid;
        payment.PaidAtUtc = now;
        payment.VerifiedByAdminUserId = adminUserId;
        payment.VerifiedAtUtc = now;
        payment.VerificationNote = "Cash collected on delivery.";
        payment.UpdatedAtUtc = now;
    }
}
