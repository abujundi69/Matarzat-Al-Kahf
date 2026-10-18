using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Entities.Content;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Entities.Promotions;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Customers;

public sealed class Customer : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string NormalizedPhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsPhoneVerified { get; set; }

    public UserStatus Status { get; set; }

    public LanguageCode PreferredLanguage { get; set; }

    public DateTime? LastLoginAtUtc { get; set; }

    public string AdministrativeNotes { get; set; } = string.Empty;

    public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();

    public ICollection<Order> Orders { get; set; } = new List<Order>();

    public ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public ICollection<CustomerSession> Sessions { get; set; } = new List<CustomerSession>();

    public ICollection<ContactMessage> ContactMessages { get; set; } = new List<ContactMessage>();
}
