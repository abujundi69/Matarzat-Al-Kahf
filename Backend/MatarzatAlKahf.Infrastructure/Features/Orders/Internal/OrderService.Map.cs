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
    private OrderDetailsResponse Map(Order order, bool includeInternalNotes) => new(order.Id, order.OrderNumber, order.CustomerId, order.CustomerName, order.PhoneNumber, ResolveDeliveryAreaName(order), order.City, order.Area, order.AddressLine, EmptyToNull(order.TaxNumber), EmptyToNull(order.DeliveryNotes), includeInternalNotes ? EmptyToNull(order.InternalNotes) : null, order.Status, order.SelectedPaymentMethod, order.Subtotal, order.DiscountAmount, order.DeliveryAmount, order.TotalAmount, order.Currency, order.CreatedAtUtc, order.Items.Select(item => new OrderItemResponse(item.ProductId, item.ProductVariantId, item.ProductNameSnapshot, item.SkuSnapshot, item.VariantDescriptionSnapshot, item.UnitPrice, item.Quantity, item.TotalPrice)).ToArray(), order.StatusHistory.OrderBy(item => item.CreatedAtUtc).Select(item => new OrderStatusResponse(item.PreviousStatus, item.NewStatus, item.Note, item.CreatedAtUtc)).ToArray(), order.Payments.OrderBy(item => item.CreatedAtUtc).Select(item => new OrderPaymentResponse(item.Id, item.Method, item.Status, item.Amount, item.Currency, EmptyToNull(item.TransactionReference), EmptyToNull(item.ProofFilePath), item.CreatedAtUtc)).ToArray());

    // Orders placed before the bilingual snapshot existed only have the English name copied into City; fall back to that so old orders still display something meaningful.
    private string ResolveDeliveryAreaName(Order order)
    {
        string arabic = string.IsNullOrEmpty(order.DeliveryAreaNameArabic) ? order.City : order.DeliveryAreaNameArabic;
        string english = string.IsNullOrEmpty(order.DeliveryAreaNameEnglish) ? order.City : order.DeliveryAreaNameEnglish;
        return languageResolver.GetCurrentLanguage() == LanguageCode.Arabic ? arabic : english;
    }
}
