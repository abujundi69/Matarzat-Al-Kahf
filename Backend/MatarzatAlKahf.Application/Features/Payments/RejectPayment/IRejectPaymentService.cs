using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.Files.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Payments.DTOs;
using MatarzatAlKahf.Application.Features.Payments.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Payments.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Payments.Interfaces;
public interface IRejectPaymentService
{
    Task<PaymentResponse> RejectAsync(Guid id, RejectPaymentRequest request, CancellationToken cancellationToken);
}
