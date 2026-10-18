using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.ContactMessages.DTOs;

namespace MatarzatAlKahf.Application.Features.ContactMessages.Interfaces;
public interface IListContactMessageService
{
    Task<PagedResult<ContactMessageResponse>> ListAsync(ContactMessageQuery query, CancellationToken cancellationToken);
}
