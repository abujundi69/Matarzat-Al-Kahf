namespace MatarzatAlKahf.Application.Common.Security;

public interface ICurrentActor
{
    Guid? AdminUserId { get; }

    Guid? CustomerId { get; }

    string ActorType { get; }

    string ActorName => string.Empty;

    /// <summary>
    /// The identifier of the token behind the current request, used to keep the caller's own
    /// session alive while revoking their other sessions.
    /// </summary>
    string SessionTokenId => string.Empty;

    string TraceId { get; }

    string IpAddress { get; }

    bool HasPermission(string permissionCode) => false;
}
