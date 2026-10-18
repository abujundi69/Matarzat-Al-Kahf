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
    private void CommitReservation(Order order)
    {
        foreach (OrderItem item in order.Items)
        {
            InventoryItem inventory = item.ProductVariant?.InventoryItem ?? throw new ConflictException("Order inventory data is unavailable.");
            if (inventory.ReservedQuantity < item.Quantity || inventory.QuantityOnHand < item.Quantity)
            {
                throw new ConflictException("Reserved inventory is inconsistent.");
            }

            inventory.ReservedQuantity -= item.Quantity;
            inventory.QuantityOnHand -= item.Quantity;
            AddInventoryTransaction(inventory, new InventoryTransaction { Type = InventoryTransactionType.Sale, Quantity = item.Quantity, Reference = order.OrderNumber, Notes = "Stock sold on order delivery." });
        }
    }
}
