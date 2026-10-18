using System.Data;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Files.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Files.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Files.Interfaces;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;
using MatarzatAlKahf.Application.Features.Notifications.Interfaces;
using MatarzatAlKahf.Application.Features.Payments.DTOs;
using MatarzatAlKahf.Application.Features.Payments.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Payments.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Payments.Interfaces;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Payments.Services;
public sealed partial class PaymentService
{
    public async Task<PaymentResponse> RejectAsync(Guid id, RejectPaymentRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length > 2000)
        {
            throw ValidationException.For(nameof(request.Reason), "A rejection reason is required and cannot exceed 2000 characters.");
        }

        return await DbTransactionExecutor.ExecuteAsync(dbContext, async token =>
        {
            Payment payment = await FindAsync(id, token);
            if (payment.Method is not (PaymentMethod.BankTransfer or PaymentMethod.ElectronicWallet)
                || payment.Status != PaymentStatus.Processing
                || string.IsNullOrWhiteSpace(payment.ProofFilePath))
            {
                throw new ConflictException("Payment is not awaiting manual verification.");
            }

            if (payment.Order.Status is not (OrderStatus.New or OrderStatus.UnderReview or OrderStatus.PaymentFailed))
            {
                throw new ConflictException("Payment cannot be rejected in the order's current state.");
            }

            DateTime now = DateTime.UtcNow;
            payment.Status = PaymentStatus.Failed;
            payment.VerificationNote = request.Reason.Trim();
            payment.VerifiedByAdminUserId = currentActor.AdminUserId;
            payment.VerifiedAtUtc = now;
            payment.UpdatedAtUtc = now;
            OrderStatus previous = payment.Order.Status;
            payment.Order.Status = OrderStatus.PaymentFailed;
            payment.Order.UpdatedAtUtc = now;
            var history = new OrderStatusHistory
            {
                PreviousStatus = previous,
                NewStatus = OrderStatus.PaymentFailed,
                ChangedByAdminUserId = currentActor.AdminUserId,
                Note = "Payment rejected."
            };
            payment.Order.StatusHistory.Add(history);
            dbContext.Set<OrderStatusHistory>().Add(history);
            await dbContext.SaveChangesAsync(token);
            await AuditAsync("payment.rejected", payment, token);
            await NotifyPaymentAsync(payment, false, token);
            return Map(payment, true);
        }, cancellationToken, IsolationLevel.Serializable);
    }
}
