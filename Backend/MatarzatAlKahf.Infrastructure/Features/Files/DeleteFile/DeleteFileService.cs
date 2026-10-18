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
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        StoredFile file = await dbContext.StoredFiles.SingleOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken) ?? throw new NotFoundException("File was not found.");
        if (await IsReferencedAsync(file, cancellationToken))
        {
            throw new ConflictException("The file is still referenced and cannot be deleted.");
        }

        await DeleteTrackedFileAsync(file, cancellationToken);
    }
}
