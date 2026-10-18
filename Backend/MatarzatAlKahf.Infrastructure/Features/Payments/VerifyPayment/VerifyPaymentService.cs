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
    public async Task<PaymentResponse> VerifyAsync(Guid id, VerifyPaymentRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        if (request.TransactionReference?.Trim().Length > 250)
        {
            errors[nameof(request.TransactionReference)] = ["Transaction reference cannot exceed 250 characters."];
        }

        if (request.Note?.Trim().Length > 2000)
        {
            errors[nameof(request.Note)] = ["Verification note cannot exceed 2000 characters."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Payment verification data is invalid.", errors);
        }

        return await DbTransactionExecutor.ExecuteAsync(dbContext, async token =>
        {
            Payment payment = await FindAsync(id, token);
            if (payment.Method is not (PaymentMethod.BankTransfer or PaymentMethod.ElectronicWallet) || payment.Status != PaymentStatus.Processing || string.IsNullOrWhiteSpace(payment.ProofFilePath))
            {
                throw new ConflictException("Payment is not awaiting manual verification.");
            }

            if (payment.Order.Status is not (OrderStatus.New or OrderStatus.UnderReview or OrderStatus.PaymentFailed))
            {
                throw new ConflictException("Payment cannot be verified in the order's current state.");
            }

            DateTime now = DateTime.UtcNow;
            payment.Status = PaymentStatus.Paid;
            if (!string.IsNullOrWhiteSpace(request.TransactionReference))
            {
                payment.TransactionReference = request.TransactionReference.Trim();
            }

            OrderStatus previous = payment.Order.Status;
            payment.VerificationNote = request.Note?.Trim() ?? string.Empty;
            payment.VerifiedByAdminUserId = currentActor.AdminUserId;
            payment.VerifiedAtUtc = now;
            payment.PaidAtUtc = now;
            payment.UpdatedAtUtc = now;
            payment.Order.Status = OrderStatus.Confirmed;
            payment.Order.UpdatedAtUtc = now;
            var history = new OrderStatusHistory
            {
                PreviousStatus = previous,
                NewStatus = OrderStatus.Confirmed,
                ChangedByAdminUserId = currentActor.AdminUserId,
                Note = "Payment verified."
            };
            payment.Order.StatusHistory.Add(history);
            dbContext.Set<OrderStatusHistory>().Add(history);

            await dbContext.SaveChangesAsync(token);
            await AuditAsync("payment.verified", payment, token);
            await NotifyPaymentAsync(payment, true, token);
            return Map(payment, true);
        }, cancellationToken, IsolationLevel.Serializable);
    }
}
