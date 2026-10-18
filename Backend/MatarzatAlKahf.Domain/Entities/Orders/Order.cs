using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Entities.Customers;
using MatarzatAlKahf.Domain.Entities.Promotions;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Orders;

public sealed class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;

    public Guid? CustomerId { get; set; }

    public Guid DeliveryLocationId { get; set; }

    // Snapshots of the delivery location's bilingual name at order time, so historical orders stay accurate after the admin renames/reprices the location.
    public string DeliveryAreaNameArabic { get; set; } = string.Empty;

    public string DeliveryAreaNameEnglish { get; set; } = string.Empty;

    public LanguageCode Language { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Area { get; set; } = string.Empty;

    public string AddressLine { get; set; } = string.Empty;

    public string TaxNumber { get; set; } = string.Empty;

    public string DeliveryNotes { get; set; } = string.Empty;

    public CurrencyCode Currency { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal DeliveryAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; }

    public PaymentMethod SelectedPaymentMethod { get; set; }

    public Guid? CouponId { get; set; }

    public string InternalNotes { get; set; } = string.Empty;

    public DateTime? InventoryReleasedAtUtc { get; set; }

    public Customer? Customer { get; set; }

    public Coupon? Coupon { get; set; }

    public DeliveryLocation DeliveryLocation { get; set; } = null!;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public CouponUsage? CouponUsage { get; set; }
}
