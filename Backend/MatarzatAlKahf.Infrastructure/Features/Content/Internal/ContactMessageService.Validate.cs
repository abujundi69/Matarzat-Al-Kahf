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
    private static void Validate(SubmitContactMessageRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        Check(request.Name, 200, nameof(request.Name), errors);
        Check(request.PhoneNumber, 30, nameof(request.PhoneNumber), errors);
        Check(request.Subject, 250, nameof(request.Subject), errors);
        Check(request.Message, 10000, nameof(request.Message), errors);
        if (!string.IsNullOrWhiteSpace(request.Email) && (request.Email.Length > 320 || !MailAddress.TryCreate(request.Email, out _)))
        {
            errors[nameof(request.Email)] = ["Email address is invalid."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Contact message is invalid.", errors);
        }
    }
}
