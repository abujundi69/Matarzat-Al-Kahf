using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.ContactMessages.DTOs;

namespace MatarzatAlKahf.Application.Features.ContactMessages.Interfaces;
public interface IReplyContactMessageService
{
    Task<ContactMessageResponse> ReplyAsync(Guid id, ReplyContactMessageRequest request, CancellationToken cancellationToken);
}
