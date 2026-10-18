using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Files.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Files.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Files.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/files")]
[Tags("Files")]
public sealed class FilesController(IUploadFileService uploadFileService, IDeleteFileService deleteFileService) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = PermissionCodes.FilesUpload)]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = 11 * 1024 * 1024)]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<ActionResult<FileResponse>> Upload(
        IFormFile file,
        bool isPublic,
        FileUploadPurpose purpose = FileUploadPurpose.Other,
        CancellationToken cancellationToken = default)
    {
        await using Stream stream = file.OpenReadStream();
        FileResponse response = await uploadFileService.UploadAsync(
            new FileUploadRequest(
                file.FileName,
                file.ContentType,
                file.Length,
                stream,
                isPublic,
                purpose),
            cancellationToken);
        return Created(response.Url, response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionCodes.FilesDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await deleteFileService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
