using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.ContactMessages.DTOs;
public sealed record SubmitContactMessageRequest(string Name, string PhoneNumber, string? Email, string Subject, string Message, LanguageCode Language = LanguageCode.Arabic, string? Website = null);
