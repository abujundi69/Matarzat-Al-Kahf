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
    public async Task<OrderDetailsResponse> LookupGuestAsync(string orderNumber, string phoneNumber, CancellationToken cancellationToken)
    {
        string normalizedPhone = phoneNormalizer.Normalize(phoneNumber);
        Order? order = await DetailsQuery().AsNoTracking().SingleOrDefaultAsync(candidate => candidate.OrderNumber == orderNumber.Trim() && candidate.PhoneNumber == normalizedPhone && candidate.CustomerId == null && !candidate.IsDeleted, cancellationToken);
        return order is null ? throw new NotFoundException("Order was not found.") : Map(order, false);
    }
}
