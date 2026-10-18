using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Orders;

public sealed class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; set; }

    public OrderStatus? PreviousStatus { get; set; }

    public OrderStatus NewStatus { get; set; }

    public Guid? ChangedByAdminUserId { get; set; }

    public string? Note { get; set; }

    public Order Order { get; set; } = null!;

    public AdminUser? AdminUser { get; set; }
}
