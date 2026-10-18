namespace MatarzatAlKahf.Application.Common.Abstractions;

public interface IAuditService
{
    Task RecordAsync(
        string action,
        string entityType,
        Guid? entityId,
        string summary,
        CancellationToken cancellationToken);
}
