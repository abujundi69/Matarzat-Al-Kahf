using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Entities.Customers;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.Content;

public sealed class ContactMessage : BaseEntity
{
    // Set when the submitter was signed in at submission time; null for guest submissions.
    public Guid? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public string Name { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public LanguageCode Language { get; set; }

    public ContactMessageStatus Status { get; set; }

    public DateTime? RepliedAtUtc { get; set; }

    public string AdministrativeReply { get; set; } = string.Empty;
}
