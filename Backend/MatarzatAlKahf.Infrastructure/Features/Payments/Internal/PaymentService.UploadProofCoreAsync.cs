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
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Payments.Services;
public sealed partial class PaymentService
{
    private async Task<PaymentResponse> UploadProofCoreAsync(Payment payment, FileUploadRequest file, CancellationToken cancellationToken)
    {
        if (payment.Method is not (PaymentMethod.BankTransfer or PaymentMethod.ElectronicWallet) || payment.Status is not (PaymentStatus.Pending or PaymentStatus.Failed))
        {
            throw new ConflictException("Payment proof cannot be uploaded for this payment.");
        }

        if (payment.Order.Status is not (OrderStatus.New or OrderStatus.UnderReview or OrderStatus.PaymentFailed))
        {
            throw new ConflictException("Payment proof cannot be uploaded in the order's current state.");
        }

        FileResponse uploaded = await fileService.UploadAsync(file with { IsPublic = false }, cancellationToken);
        payment.ProofFilePath = uploaded.Url;
        payment.Status = PaymentStatus.Processing;
        payment.VerificationNote = string.Empty;
        payment.UpdatedAtUtc = DateTime.UtcNow;
        if (payment.Order.Status != OrderStatus.UnderReview)
        {
            OrderStatus previous = payment.Order.Status;
            payment.Order.Status = OrderStatus.UnderReview;
            payment.Order.UpdatedAtUtc = DateTime.UtcNow;
            var history = new OrderStatusHistory
            {
                PreviousStatus = previous,
                NewStatus = OrderStatus.UnderReview,
                Note = "Payment proof submitted for review."
            };
            payment.Order.StatusHistory.Add(history);
            dbContext.OrderStatusHistory.Add(history);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(payment, false);
    }
}
