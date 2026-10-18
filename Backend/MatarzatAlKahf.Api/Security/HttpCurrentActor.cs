using System.Security.Claims;
using MatarzatAlKahf.Application.Common.Security;

namespace MatarzatAlKahf.Api.Security;

internal sealed class HttpCurrentActor(
    IHttpContextAccessor httpContextAccessor) : ICurrentActor
{
    private HttpContext? Context => httpContextAccessor.HttpContext;

    public Guid? AdminUserId => GetGuid(AdminClaimTypes.AdminUserId);

    public Guid? CustomerId => GetGuid("customer_id");

    public string ActorName => Context?.User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

    public string ActorType => AdminUserId.HasValue
        ? "Admin"
        : CustomerId.HasValue ? "Customer" : "System";

    public string SessionTokenId => Context?.User.FindFirstValue(
        System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti) ?? string.Empty;

    public string TraceId => Context?.TraceIdentifier ?? string.Empty;

    public string IpAddress =>
        Context?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

    public bool HasPermission(string permissionCode) =>
        !string.IsNullOrWhiteSpace(permissionCode)
        && Context?.User.HasClaim(AdminClaimTypes.Permission, permissionCode) == true;

    private Guid? GetGuid(string claimType)
    {
        return Guid.TryParse(
            Context?.User.FindFirstValue(claimType),
            out Guid value)
            ? value
            : null;
    }
}
