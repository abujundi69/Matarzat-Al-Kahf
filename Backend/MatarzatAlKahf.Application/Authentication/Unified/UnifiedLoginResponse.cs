namespace MatarzatAlKahf.Application.Authentication.Unified;

public sealed record UnifiedLoginResponse(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAtUtc,
    AuthenticatedActorDto Actor);
