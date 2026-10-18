using System.Security.Cryptography;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Files.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Files.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Files.Interfaces;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Files.Services;
public sealed partial class FileService
{
    private static FileResponse Map(StoredFile file) => new(
        file.Id,
        file.OriginalFileName,
        file.ContentType,
        file.Length,
        file.IsPublic
            ? $"/uploads/{file.StorageKey.Replace('\\', '/')}"
            : $"/api/v1/files/{file.Id}",
        file.CreatedAtUtc);
}
