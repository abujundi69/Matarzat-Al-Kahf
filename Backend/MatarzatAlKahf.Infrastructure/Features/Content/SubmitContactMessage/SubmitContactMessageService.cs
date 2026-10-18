using System.Net.Mail;
using MatarzatAlKahf.Application.Common.Exceptions;
using MatarzatAlKahf.Application.Common.Normalization;
using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.ContactMessages.DTOs;
using MatarzatAlKahf.Application.Features.ContactMessages.Interfaces;
using MatarzatAlKahf.Domain.Entities.Content;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Content.Services;
public sealed partial class ContactMessageService
{
    public async Task SubmitAsync(SubmitContactMessageRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            return;
        }

        dbContext.ContactMessages.Add(new ContactMessage { CustomerId = currentActor.CustomerId, Name = request.Name.Trim(), PhoneNumber = phoneNormalizer.Normalize(request.PhoneNumber), Email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty, Subject = request.Subject.Trim(), Message = request.Message.Trim(), Language = request.Language, Status = ContactMessageStatus.New });
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
