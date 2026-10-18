using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Application.Authentication.Admin;
public sealed record AdminLoginResponse(string AccessToken, string TokenType, DateTime ExpiresAtUtc, AdminProfileDto Admin);
