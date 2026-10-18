using MatarzatAlKahf.Application.Features.Files.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MatarzatAlKahf.Infrastructure.Features.Files.Services;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _rootPath;
    private readonly string? _legacyRootPath;

    public LocalFileStorage(
        IOptions<FileStorageOptions> options,
        IHostEnvironment hostEnvironment)
    {
        _rootPath = FileStorageOptions.ResolveRootPath(
            options.Value.RootPath,
            hostEnvironment.ContentRootPath);
        string legacyRootPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "uploads"));
        _legacyRootPath = string.Equals(
            _rootPath,
            legacyRootPath,
            StringComparison.OrdinalIgnoreCase)
            ? null
            : legacyRootPath;
        Directory.CreateDirectory(_rootPath);
        Directory.CreateDirectory(Path.Combine(_rootPath, "private"));
        foreach (string folder in FileStorageOptions.PublicFolders)
        {
            Directory.CreateDirectory(Path.Combine(_rootPath, folder));
        }
    }

    public async Task StoreAsync(
        string key,
        Stream content,
        CancellationToken cancellationToken)
    {
        string path = Resolve(key);
        string? directory = Path.GetDirectoryName(path);
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        string temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await using (var output = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await content.CopyToAsync(output, cancellationToken);
            }

            File.Move(temporaryPath, path, false);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string path = Resolve(key);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        string? legacyPath = ResolveLegacy(key);
        if (legacyPath is not null && File.Exists(legacyPath))
        {
            File.Delete(legacyPath);
        }

        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(string key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string path = Resolve(key);
        if (!File.Exists(path))
        {
            string? legacyPath = ResolveLegacy(key);
            if (legacyPath is not null && File.Exists(legacyPath))
            {
                path = legacyPath;
            }
        }

        Stream stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    private string Resolve(string key)
    {
        return ResolveUnderRoot(_rootPath, key);
    }

    private string? ResolveLegacy(string key)
    {
        return _legacyRootPath is null
            ? null
            : ResolveUnderRoot(_legacyRootPath, key);
    }

    private static string ResolveUnderRoot(string rootPath, string key)
    {
        if (string.IsNullOrWhiteSpace(key)
            || Path.IsPathRooted(key)
            || key.Contains('\0'))
        {
            throw new InvalidOperationException("Invalid file storage key.");
        }

        string normalized = key.Replace('/', Path.DirectorySeparatorChar);
        string fullPath = Path.GetFullPath(Path.Combine(rootPath, normalized));
        string rootPrefix = rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? rootPath
            : rootPath + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid file storage key.");
        }

        return fullPath;
    }
}
