using MatarzatAlKahf.Domain.Common;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Domain.Entities.System;

public sealed class ImportOperation : BaseEntity
{
    public ImportType Type { get; set; }
    public ImportStatus Status { get; set; }
    public Guid AdminUserId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileHash { get; set; } = string.Empty;
    public string RowsJson { get; set; } = string.Empty;
    public string ErrorsJson { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public string CommitIdempotencyKey { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public int InsertedRows { get; set; }
    public int UpdatedRows { get; set; }
    public AdminUser AdminUser { get; set; } = null!;
}
