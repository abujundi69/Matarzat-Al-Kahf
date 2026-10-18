using System.Data;
using System.Net;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Localization;
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
public sealed partial class OrderService(MatarzatAlKahfDbContext dbContext, ICurrentActor currentActor, IPhoneNumberNormalizer phoneNormalizer, IAuditService auditService, INotificationService notificationService, IActiveLanguageResolver languageResolver) : IOrderService
{
    private static Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions { get; } = new Dictionary<OrderStatus, OrderStatus[]>
    {
        [OrderStatus.New] = [OrderStatus.UnderReview, OrderStatus.Confirmed, OrderStatus.Cancelled, OrderStatus.PaymentFailed],
        [OrderStatus.UnderReview] = [OrderStatus.Confirmed, OrderStatus.Cancelled],
        [OrderStatus.Confirmed] = [OrderStatus.Preparing, OrderStatus.Cancelled],
        [OrderStatus.Preparing] = [OrderStatus.ReadyForShipping, OrderStatus.Cancelled],
        [OrderStatus.ReadyForShipping] = [OrderStatus.Shipped, OrderStatus.Cancelled],
        [OrderStatus.Shipped] = [OrderStatus.Delivered, OrderStatus.Returned],
        [OrderStatus.Delivered] = [OrderStatus.Returned],
        [OrderStatus.PaymentFailed] = [OrderStatus.Cancelled]
    };
}
