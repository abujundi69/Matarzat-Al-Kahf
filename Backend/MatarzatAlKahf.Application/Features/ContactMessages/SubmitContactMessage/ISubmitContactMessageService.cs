using MatarzatAlKahf.Application.Common.Pagination;
using MatarzatAlKahf.Application.Features.ContactMessages.DTOs;

namespace MatarzatAlKahf.Application.Features.ContactMessages.Interfaces;
public interface ISubmitContactMessageService
{
    Task SubmitAsync(SubmitContactMessageRequest request, CancellationToken cancellationToken);
}
