using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Exports.DTOs;
using MatarzatAlKahf.Application.Features.Exports.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/exports")]
[Tags("Exports")]
public sealed class ExportsController(IOrdersExportService ordersExportService, ISalesExportService salesExportService, IProductsExportService productsExportService, ICustomersExportService customersExportService, IInventoryExportService inventoryExportService) : ControllerBase
{
    [HttpGet("orders")]
    [Authorize(Policy = PermissionCodes.OrdersExport)]
    public async Task<IActionResult> Orders([FromQuery] ExportQuery query, CancellationToken cancellationToken) => ToFile(await ordersExportService.OrdersAsync(query, cancellationToken));
    [HttpGet("sales")]
    [Authorize(Policy = PermissionCodes.ReportsExport)]
    public async Task<IActionResult> Sales([FromQuery] ExportQuery query, CancellationToken cancellationToken) => ToFile(await salesExportService.SalesAsync(query, cancellationToken));
    [HttpGet("products")]
    [Authorize(Policy = PermissionCodes.ReportsExport)]
    public async Task<IActionResult> Products([FromQuery] ExportQuery query, CancellationToken cancellationToken) => ToFile(await productsExportService.ProductsAsync(query, cancellationToken));
    [HttpGet("customers")]
    [Authorize(Policy = PermissionCodes.ReportsExport)]
    public async Task<IActionResult> Customers([FromQuery] ExportQuery query, CancellationToken cancellationToken) => ToFile(await customersExportService.CustomersAsync(query, cancellationToken));
    [HttpGet("inventory")]
    [Authorize(Policy = PermissionCodes.ReportsExport)]
    public async Task<IActionResult> Inventory([FromQuery] ExportQuery query, CancellationToken cancellationToken) => ToFile(await inventoryExportService.InventoryAsync(query, cancellationToken));
    private FileStreamResult ToFile(ExportFile file) => File(file.Content, file.ContentType, file.FileName);
}
