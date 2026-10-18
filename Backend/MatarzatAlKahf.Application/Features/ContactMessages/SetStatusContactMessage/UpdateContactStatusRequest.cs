using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.ContactMessages.DTOs;
public sealed record UpdateContactStatusRequest(ContactMessageStatus Status);
