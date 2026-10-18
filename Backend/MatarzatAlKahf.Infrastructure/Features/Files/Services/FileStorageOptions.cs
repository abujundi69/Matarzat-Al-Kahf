namespace MatarzatAlKahf.Infrastructure.Features.Files.Services;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public const string DefaultRelativeRootPath = "../uploads";

    public static readonly IReadOnlyList<string> PublicFolders =
    [
        "logos",
        "products",
        "categories",
        "banners",
        "content",
        "other"
    ];

    public string RootPath { get; set; } = string.Empty;

    public static string ResolveRootPath(string? configuredPath, string contentRootPath)
    {
        string path = string.IsNullOrWhiteSpace(configuredPath)
            ? DefaultRelativeRootPath
            : configuredPath.Trim();
        return Path.GetFullPath(
            Path.IsPathRooted(path)
                ? path
                : Path.Combine(contentRootPath, path));
    }
}
