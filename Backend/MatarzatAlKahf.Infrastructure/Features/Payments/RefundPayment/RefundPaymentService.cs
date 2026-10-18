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
    public async Task<PaymentResponse> RefundAsync(Guid id, RefundPaymentRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length > 2000)
        {
            throw ValidationException.For(nameof(request.Reason), "A refund reason is required and cannot exceed 2000 characters.");
        }

        return await DbTransactionExecutor.ExecuteAsync(dbContext, async token =>
        {
            Payment payment = await FindAsync(id, token);
            if (payment.Status != PaymentStatus.Paid)
            {
                throw new ConflictException("Only a paid payment can be marked as refunded.");
            }

            decimal amount = request.Amount ?? payment.Amount;
            if (amount <= 0 || amount > payment.Amount)
            {
                throw ValidationException.For(nameof(request.Amount), "Refund amount is invalid.");
            }

            payment.Status = amount == payment.Amount ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
            payment.VerificationNote = request.Reason.Trim();
            payment.VerifiedByAdminUserId = currentActor.AdminUserId;
            payment.VerifiedAtUtc = DateTime.UtcNow;
            payment.UpdatedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(token);
            await AuditAsync("payment.refund.recorded", payment, token);
            return Map(payment, true);
        }, cancellationToken, IsolationLevel.Serializable);
    }
}
