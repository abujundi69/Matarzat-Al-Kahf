namespace MatarzatAlKahf.Application.Authentication.Unified;

public interface IUnifiedAuthenticationService
{
    Task<UnifiedLoginResponse> LoginAsync(
        UnifiedLoginRequest request,
        CancellationToken cancellationToken);
}
