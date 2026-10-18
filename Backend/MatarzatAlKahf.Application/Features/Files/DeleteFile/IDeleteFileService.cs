using MatarzatAlKahf.Application.Features.Files.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Files.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Files.Interfaces;
public interface IDeleteFileService
{
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
