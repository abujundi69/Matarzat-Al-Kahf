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
    private static ContactMessageResponse Map(ContactMessage message) => new(message.Id, message.Name, message.PhoneNumber, string.IsNullOrEmpty(message.Email) ? null : message.Email, message.Subject, message.Message, message.Language, message.Status, string.IsNullOrEmpty(message.AdministrativeReply) ? null : message.AdministrativeReply, message.RepliedAtUtc, message.CreatedAtUtc);
}
