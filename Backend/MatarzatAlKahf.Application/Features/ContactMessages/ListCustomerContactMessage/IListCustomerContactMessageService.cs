using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.ContactMessages.DTOs;

namespace MatarzatAlKahf.Application.Features.ContactMessages.Interfaces;
public interface IListCustomerContactMessageService
{
    Task<PagedResult<ContactMessageResponse>> ListCustomerAsync(int page, int pageSize, CancellationToken cancellationToken);
}
