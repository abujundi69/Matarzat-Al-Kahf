using System.Data;
using System.Net;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Notifications.DTOs;
using MatarzatAlKahf.Application.Features.Notifications.Interfaces;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Orders.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Orders.Interfaces;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Execution;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Orders.Services;
public sealed partial class OrderService
{
    public async Task<string> GetCustomerInvoiceHtmlAsync(Guid id, CancellationToken cancellationToken)
    {
        OrderDetailsResponse order = await GetCustomerAsync(id, cancellationToken);
        string rows = string.Join(string.Empty, order.Items.Select(item => $"<tr><td>{WebUtility.HtmlEncode(item.Name)}</td><td>{WebUtility.HtmlEncode(item.Sku)}</td><td>{item.Quantity}</td><td>{item.UnitPrice:0.00}</td><td>{item.Total:0.00}</td></tr>"));
        return "<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\">" + $"<title>Invoice {WebUtility.HtmlEncode(order.OrderNumber)}</title>" + "<style>body{font-family:Arial,sans-serif;max-width:900px;margin:40px auto}" + "table{width:100%;border-collapse:collapse}th,td{padding:8px;border:1px solid #ddd;text-align:start}.totals{text-align:end}</style>" + "</head><body><h1>Invoice / فاتورة</h1>" + $"<p><strong>{WebUtility.HtmlEncode(order.OrderNumber)}</strong><br>{order.CreatedAtUtc:u}</p>" + $"<p>{WebUtility.HtmlEncode(order.CustomerName)}<br>{WebUtility.HtmlEncode(order.PhoneNumber)}<br>{WebUtility.HtmlEncode(order.City)}, {WebUtility.HtmlEncode(order.Area)}, {WebUtility.HtmlEncode(order.AddressLine)}</p>" + $"<table><thead><tr><th>Item</th><th>SKU</th><th>Qty</th><th>Price</th><th>Total</th></tr></thead><tbody>{rows}</tbody></table>" + $"<div class=\"totals\"><p>Subtotal: {order.Subtotal:0.00} {order.Currency}</p><p>Discount: {order.Discount:0.00}</p><p>Delivery: {order.Delivery:0.00}</p><h2>Total: {order.Total:0.00} {order.Currency}</h2></div>" + "</body></html>";
    }
}
