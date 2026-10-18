using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.AdminUsers.DTOs.Responses;
public sealed record AdminRoleSummaryResponse(Guid Id, string Name)
{
    public string? DisplayNameArabic { get; init; }

    public string? DisplayNameEnglish { get; init; }
}
