using MatarzatAlKahf.Application.Features.Files.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Payments.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Payments.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MatarzatAlKahf.Api.Controllers.V1.Storefront;
[ApiController]
[AllowAnonymous]
[Route("api/v1/orders/{orderId:guid}/payment-proof")]
[Tags("Payments")]
public sealed class GuestPaymentsController(IUploadGuestProofPaymentService uploadGuestProofPaymentService) : ControllerBase
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    [EnableRateLimiting("GuestOrderLookup")]
    public async Task<ActionResult<PaymentResponse>> Upload(Guid orderId, [FromForm] Guid paymentId, [FromForm] string phoneNumber, IFormFile file, CancellationToken cancellationToken)
    {
        await using Stream stream = file.OpenReadStream();
        return Ok(await uploadGuestProofPaymentService.UploadGuestProofAsync(orderId, paymentId, phoneNumber, new FileUploadRequest(file.FileName, file.ContentType, file.Length, stream), cancellationToken));
    }
}
