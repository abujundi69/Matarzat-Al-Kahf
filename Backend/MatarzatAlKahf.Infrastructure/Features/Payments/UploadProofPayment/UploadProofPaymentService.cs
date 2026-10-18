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
    public async Task<PaymentResponse> UploadProofAsync(Guid orderId, Guid paymentId, FileUploadRequest file, CancellationToken cancellationToken)
    {
        Guid customerId = currentActor.CustomerId ?? throw new AuthenticationFailedException("Customer authentication is required.");
        Payment payment = await dbContext.Payments
            .Include(item => item.Order)
            .SingleOrDefaultAsync(
                item => item.Id == paymentId
                    && item.OrderId == orderId
                    && item.Order.CustomerId == customerId
                    && !item.Order.IsDeleted
                    && !item.IsDeleted,
                cancellationToken)
            ?? throw new NotFoundException("Payment was not found.");
        return await UploadProofCoreAsync(payment, file, cancellationToken);
    }
}
