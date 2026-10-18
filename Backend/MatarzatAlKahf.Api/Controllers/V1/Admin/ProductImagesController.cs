using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Files.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Files.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Files.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/products/{productId:guid}/images")]
[Tags("Files")]
public sealed class ProductImagesController(IUploadProductImageService uploadProductImageService, IDeleteProductImageService deleteProductImageService) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = PermissionCodes.FilesUpload)]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<FileResponse>> Upload(Guid productId, IFormFile file, string? arabicAltText, string? englishAltText, int sortOrder, bool isPrimary, CancellationToken cancellationToken)
    {
        await using Stream stream = file.OpenReadStream();
        FileResponse response = await uploadProductImageService.UploadAsync(productId, new ProductImageUploadRequest(new FileUploadRequest(file.FileName, file.ContentType, file.Length, stream, true, FileUploadPurpose.Products), arabicAltText, englishAltText, sortOrder, isPrimary), cancellationToken);
        return Created(response.Url, response);
    }

    [HttpDelete("{imageId:guid}")]
    [Authorize(Policy = PermissionCodes.FilesDelete)]
    public async Task<IActionResult> Delete(Guid productId, Guid imageId, CancellationToken cancellationToken)
    {
        await deleteProductImageService.DeleteAsync(productId, imageId, cancellationToken);
        return NoContent();
    }
}
