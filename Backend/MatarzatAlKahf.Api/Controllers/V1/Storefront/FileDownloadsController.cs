using MatarzatAlKahf.Application.Features.Files.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Files.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Storefront;
[ApiController]
[Route("api/v1/files")]
[AllowAnonymous]
[Tags("Files")]
public sealed class FileDownloadsController(IDownloadFileService downloadFileService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        FileDownload file = await downloadFileService.DownloadAsync(id, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName, enableRangeProcessing: true);
    }
}
