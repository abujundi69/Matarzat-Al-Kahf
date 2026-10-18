using MatarzatAlKahf.Domain.Common;

namespace MatarzatAlKahf.Domain.Entities.System;

public sealed class StoredFile : BaseEntity
{
    public string StorageKey { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Length { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public Guid? UploadedByAdminUserId { get; set; }
    public Guid? UploadedByCustomerId { get; set; }
    public bool IsPublic { get; set; }
}
