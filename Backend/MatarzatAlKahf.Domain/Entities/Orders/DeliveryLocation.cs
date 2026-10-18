using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Entities.Customers;

namespace MatarzatAlKahf.Domain.Entities.Orders;

public sealed class DeliveryLocation : BaseEntity
{
    public string CountryCode { get; set; } = string.Empty;
    public string CountryNameArabic { get; set; } = string.Empty;
    public string CountryNameEnglish { get; set; } = string.Empty;
    public string NameArabic { get; set; } = string.Empty;
    public string NameEnglish { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool CashOnDeliveryAllowed { get; set; }
    public bool ProductPriceIncludesDelivery { get; set; }
    public decimal AdditionalDeliveryCost { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public int SortOrder { get; set; }
    public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
}
