using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.ContactMessages.DTOs;
using MatarzatAlKahf.Domain.Entities.Content;
using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Features.Content.Services;
public sealed partial class ContactMessageService
{
    public async Task<PagedResult<ContactMessageResponse>> ListCustomerAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        PaginationRules.Validate(page, pageSize);
        Guid customerId = RequireCustomer();
        IQueryable<ContactMessage> messages = dbContext.ContactMessages.Where(message => message.CustomerId == customerId && !message.IsDeleted);
        int count = await messages.CountAsync(cancellationToken);
        List<ContactMessage> items = await messages.AsNoTracking().OrderByDescending(message => message.CreatedAtUtc).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return PagedResultFactory.Create(items.Select(Map).ToArray(), page, pageSize, count);
    }
}
