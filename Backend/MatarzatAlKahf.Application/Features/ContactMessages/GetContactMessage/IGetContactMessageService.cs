using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.ContactMessages.DTOs;

namespace MatarzatAlKahf.Application.Features.ContactMessages.Interfaces;
public interface IGetContactMessageService
{
    Task<ContactMessageResponse> GetAsync(Guid id, CancellationToken cancellationToken);
}
