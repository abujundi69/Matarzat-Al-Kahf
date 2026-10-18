using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Files.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Payments.DTOs;
using MatarzatAlKahf.Application.Features.Payments.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Payments.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Customer;
[ApiController]
[Route("api/v1/customer/orders/{orderId:guid}/payments")]
[Authorize(Policy = AuthenticationPolicyNames.Customer)]
[Tags("Payments")]
public sealed class CustomerPaymentsController(IGetCustomerHistoryPaymentService getCustomerHistoryPaymentService, IUploadProofPaymentService uploadProofPaymentService, IGetCustomerOrderPaymentMethodService getCustomerOrderPaymentMethodService) : ControllerBase
{
    [HttpGet("instructions")]
    public async Task<ActionResult<PaymentMethodResponse>> Instructions(Guid orderId, CancellationToken cancellationToken) => Ok(await getCustomerOrderPaymentMethodService.GetCustomerOrderAsync(orderId, cancellationToken));
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PaymentResponse>>> History(Guid orderId, CancellationToken cancellationToken) => Ok(await getCustomerHistoryPaymentService.GetCustomerHistoryAsync(orderId, cancellationToken));
    [HttpPost("{paymentId:guid}/proof")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PaymentResponse>> UploadProof(Guid orderId, Guid paymentId, IFormFile file, CancellationToken cancellationToken)
    {
        await using Stream stream = file.OpenReadStream();
        return Ok(await uploadProofPaymentService.UploadProofAsync(orderId, paymentId, new FileUploadRequest(file.FileName, file.ContentType, file.Length, stream), cancellationToken));
    }
}
