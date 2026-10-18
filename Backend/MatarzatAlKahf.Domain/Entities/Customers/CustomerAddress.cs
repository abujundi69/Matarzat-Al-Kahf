using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Customers;

public sealed class CustomerAddress : BaseEntity
{
    public Guid CustomerId { get; set; }

    public Guid DeliveryLocationId { get; set; }

    public AddressType Type { get; set; }

    public string City { get; set; } = string.Empty;

    public string Area { get; set; } = string.Empty;

    public string AddressLine { get; set; } = string.Empty;

    public string TaxNumber { get; set; } = string.Empty;

    public string DeliveryNotes { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public Customer Customer { get; set; } = null!;

    public DeliveryLocation DeliveryLocation { get; set; } = null!;
}
