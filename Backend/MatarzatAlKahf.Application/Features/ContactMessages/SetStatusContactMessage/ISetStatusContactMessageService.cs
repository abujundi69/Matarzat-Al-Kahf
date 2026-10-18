using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.ContactMessages.DTOs;

namespace MatarzatAlKahf.Application.Features.ContactMessages.Interfaces;
public interface ISetStatusContactMessageService
{
    Task<ContactMessageResponse> SetStatusAsync(Guid id, UpdateContactStatusRequest request, CancellationToken cancellationToken);
}
