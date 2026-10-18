using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Reports.DTOs;
using MatarzatAlKahf.Application.Features.Reports.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MatarzatAlKahf.Api.Controllers.V1.Admin;
[ApiController]
[Route("api/v1/admin/reports")]
[Tags("Reports")]
public sealed class ReportsController(ISalesReportService salesReportService, IProductsReportService productsReportService, ICustomersReportService customersReportService, IOrdersReportService ordersReportService, IInventoryReportService inventoryReportService) : ControllerBase
{
    [HttpGet("sales")]
    [Authorize(Policy = PermissionCodes.ReportsSalesRead)]
    public async Task<ActionResult<SalesReportResponse>> Sales([FromQuery] ReportQuery query, CancellationToken cancellationToken) => Ok(await salesReportService.SalesAsync(query, cancellationToken));
    [HttpGet("products")]
    [Authorize(Policy = PermissionCodes.ReportsProductsRead)]
    public async Task<ActionResult<ProductReportResponse>> Products([FromQuery] ReportQuery query, CancellationToken cancellationToken) => Ok(await productsReportService.ProductsAsync(query, cancellationToken));
    [HttpGet("customers")]
    [Authorize(Policy = PermissionCodes.ReportsCustomersRead)]
    public async Task<ActionResult<CustomerReportResponse>> Customers([FromQuery] ReportQuery query, CancellationToken cancellationToken) => Ok(await customersReportService.CustomersAsync(query, cancellationToken));
    [HttpGet("orders")]
    [Authorize(Policy = PermissionCodes.ReportsOrdersRead)]
    public async Task<ActionResult<OrderReportResponse>> Orders([FromQuery] ReportQuery query, CancellationToken cancellationToken) => Ok(await ordersReportService.OrdersAsync(query, cancellationToken));
    [HttpGet("inventory")]
    [Authorize(Policy = PermissionCodes.ReportsInventoryRead)]
    public async Task<ActionResult<InventoryReportResponse>> Inventory([FromQuery] ReportQuery query, CancellationToken cancellationToken) => Ok(await inventoryReportService.InventoryAsync(query, cancellationToken));
}
