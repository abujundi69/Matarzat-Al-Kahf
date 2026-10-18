using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.ContactMessages.DTOs;
public sealed record ContactMessageQuery(int Page = 1, int PageSize = 20, string? Search = null, ContactMessageStatus? Status = null);
