using MatarzatAlKahf.Application.Features.Files.DTOs.Requests;
using MatarzatAlKahf.Application.Features.Files.DTOs.Responses;

namespace MatarzatAlKahf.Application.Features.Files.Interfaces;
public interface IUploadProductImageService
{
    Task<FileResponse> UploadAsync(Guid productId, ProductImageUploadRequest request, CancellationToken cancellationToken);
}
