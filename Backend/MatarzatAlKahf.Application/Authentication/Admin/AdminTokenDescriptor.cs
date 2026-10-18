using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Authentication.Admin;
public sealed record AdminTokenDescriptor(Guid AdminUserId, string FullName, string PhoneNumber, LanguageCode PreferredLanguage, IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions);
