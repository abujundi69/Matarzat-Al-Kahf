namespace MatarzatAlKahf.Application.Features.Files.Interfaces;

public interface IFileStorage
{
    Task StoreAsync(string key, Stream content, CancellationToken cancellationToken);

    Task DeleteAsync(string key, CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken);
}
