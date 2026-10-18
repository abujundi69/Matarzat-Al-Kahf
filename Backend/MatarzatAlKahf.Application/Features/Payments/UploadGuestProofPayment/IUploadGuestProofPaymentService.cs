using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Files.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Payments.DTOs;
using MatarzatAlKahf.Application.Features.Payments.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Payments.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Payments.Interfaces;
public interface IUploadGuestProofPaymentService
{
    Task<PaymentResponse> UploadGuestProofAsync(Guid orderId, Guid paymentId, string phoneNumber, FileUploadRequest file, CancellationToken cancellationToken);
}
