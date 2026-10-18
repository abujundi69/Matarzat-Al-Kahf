using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Features.CustomerAuthentication.Interfaces;
public sealed record CustomerTokenDescriptor(Guid CustomerId, string FullName, string PhoneNumber, LanguageCode PreferredLanguage);
