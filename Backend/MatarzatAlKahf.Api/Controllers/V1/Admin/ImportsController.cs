using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.DataImports.DTOs;
using MatarzatAlKahf.Application.Features.DataImports.Interfaces;
using MatarzatAlKahf.Application.Features.Exports.DTOs;
using MatarzatAlKahf.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Authorize]
[Route("api/v1/admin/imports")]
[Tags("Imports")]
public sealed class ImportsController(ICreateTemplateImportService createTemplateImportService, IPreviewImportService previewImportService, ICommitImportService commitImportService) : ControllerBase
{
    [HttpGet("products/template")]
    [Authorize(Policy = PermissionCodes.ImportsProducts)]
    public IActionResult ProductTemplate([FromQuery] LanguageCode language = LanguageCode.Arabic) => ToFile(createTemplateImportService.CreateTemplate(ImportType.Products, language));
    [HttpPost("products/preview")]
    [Authorize(Policy = PermissionCodes.ImportsProducts)]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public Task<ActionResult<ImportPreviewResponse>> PreviewProducts(IFormFile file, [FromQuery] bool allowNewProducts, CancellationToken cancellationToken) => Preview(ImportType.Products, file, allowNewProducts, cancellationToken);
    [HttpPost("products/commit")]
    [Authorize(Policy = PermissionCodes.ImportsProducts)]
    public Task<ActionResult<ImportCommitResponse>> CommitProducts([FromQuery] Guid importId, [FromBody] CommitImportRequest request, [FromHeader(Name = "Idempotency-Key")] string idempotencyKey, CancellationToken cancellationToken) => Commit(importId, ImportType.Products, request, idempotencyKey, cancellationToken);
    [HttpGet("inventory/template")]
    [Authorize(Policy = PermissionCodes.ImportsInventory)]
    public IActionResult InventoryTemplate([FromQuery] LanguageCode language = LanguageCode.Arabic) => ToFile(createTemplateImportService.CreateTemplate(ImportType.Inventory, language));
    [HttpPost("inventory/preview")]
    [Authorize(Policy = PermissionCodes.ImportsInventory)]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public Task<ActionResult<ImportPreviewResponse>> PreviewInventory(IFormFile file, CancellationToken cancellationToken) => Preview(ImportType.Inventory, file, false, cancellationToken);
    [HttpPost("inventory/commit")]
    [Authorize(Policy = PermissionCodes.ImportsInventory)]
    public Task<ActionResult<ImportCommitResponse>> CommitInventory([FromQuery] Guid importId, [FromBody] CommitImportRequest request, [FromHeader(Name = "Idempotency-Key")] string idempotencyKey, CancellationToken cancellationToken) => Commit(importId, ImportType.Inventory, request, idempotencyKey, cancellationToken);
    private async Task<ActionResult<ImportPreviewResponse>> Preview(ImportType type, IFormFile file, bool allowNewProducts, CancellationToken cancellationToken)
    {
        await using Stream content = file.OpenReadStream();
        var result = await previewImportService.PreviewAsync(type, new ImportUploadRequest(file.FileName, file.Length, content, allowNewProducts), cancellationToken);
        return Ok(result);
    }

    private async Task<ActionResult<ImportCommitResponse>> Commit(Guid importId, ImportType type, CommitImportRequest request, string idempotencyKey, CancellationToken cancellationToken)
    {
        var result = await commitImportService.CommitAsync(importId, type, request, idempotencyKey, cancellationToken);
        return Ok(result);
    }

    private FileStreamResult ToFile(ExportFile file) => File(file.Content, file.ContentType, file.FileName);
}
