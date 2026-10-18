using System.Security.Cryptography;
using MatarzatAlKahf.Application.Common.Abstractions;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Security;
using MatarzatAlKahf.Application.Features.Files.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Files.DTOs.Responses;
using MatarzatAlKahf.Application.Features.Files.Interfaces;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Files.Services;
public sealed partial class FileService
{
    public async Task<FileDownload> DownloadAsync(Guid id, CancellationToken cancellationToken)
    {
        StoredFile file = await dbContext.StoredFiles.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken) ?? throw new NotFoundException("File was not found.");
        if (!file.IsPublic)
        {
            bool customerOwnsFile = currentActor.CustomerId.HasValue
                && currentActor.CustomerId == file.UploadedByCustomerId;
            bool adminOwnsFile = currentActor.AdminUserId.HasValue
                && currentActor.AdminUserId == file.UploadedByAdminUserId;
            bool isReviewablePaymentProof = false;
            if (!customerOwnsFile
                && !adminOwnsFile
                && currentActor.AdminUserId.HasValue
                && currentActor.HasPermission(PermissionCodes.PaymentsRead))
            {
                string canonicalUrl = $"/api/v1/files/{file.Id}";
                isReviewablePaymentProof = await dbContext.Payments
                    .AsNoTracking()
                    .AnyAsync(
                        payment => !payment.IsDeleted
                            && !payment.Order.IsDeleted
                            && (payment.Method == PaymentMethod.BankTransfer
                                || payment.Method == PaymentMethod.ElectronicWallet)
                            && payment.ProofFilePath == canonicalUrl,
                        cancellationToken);
            }

            if (!customerOwnsFile && !adminOwnsFile && !isReviewablePaymentProof)
            {
                throw new NotFoundException("File was not found.");
            }
        }

        return new FileDownload(await storage.OpenReadAsync(file.StorageKey, cancellationToken), file.ContentType, file.OriginalFileName);
    }
}
