using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.ContactMessages.DTOs;
public sealed record ContactMessageResponse(Guid Id, string Name, string PhoneNumber, string? Email, string Subject, string Message, LanguageCode Language, ContactMessageStatus Status, string? AdministrativeReply, DateTime? RepliedAtUtc, DateTime CreatedAtUtc);
