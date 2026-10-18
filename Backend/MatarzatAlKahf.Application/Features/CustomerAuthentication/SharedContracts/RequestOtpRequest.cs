using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.DTOs.Requests;

public sealed record RequestOtpRequest(
    string PhoneNumber,
    LanguageCode Language = LanguageCode.Arabic);
