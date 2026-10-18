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
    public async Task<PagedResult<ContactMessageResponse>> ListAsync(ContactMessageQuery query, CancellationToken cancellationToken)
    {
        PaginationRules.Validate(query.Page, query.PageSize);
        IQueryable<ContactMessage> messages = dbContext.ContactMessages.Where(message => !message.IsDeleted);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search = query.Search.Trim();
            messages = messages.Where(message => message.Name.Contains(search) || message.PhoneNumber.Contains(search) || message.Email.Contains(search) || message.Subject.Contains(search));
        }

        if (query.Status.HasValue)
        {
            messages = messages.Where(message => message.Status == query.Status);
        }

        int count = await messages.CountAsync(cancellationToken);
        List<ContactMessage> page = await messages.AsNoTracking().OrderByDescending(message => message.CreatedAtUtc).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(cancellationToken);
        return PagedResultFactory.Create(page.Select(Map).ToArray(), query.Page, query.PageSize, count);
    }
}
