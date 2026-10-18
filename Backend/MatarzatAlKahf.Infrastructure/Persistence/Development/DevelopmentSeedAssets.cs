using System.Text.Json;

namespace MatarzatAlKahf.Infrastructure.Persistence.Development;

/// <summary>
/// Installs the versioned demo photographs into the application's existing
/// public upload folders. No network access is required at migration or startup.
/// </summary>
public static class DevelopmentSeedAssets
{
    public static void EnsureAvailable(string contentRootPath, string uploadsRootPath)
    {
        string sourceRoot = Path.Combine(contentRootPath, "SeedAssets");
        if (!File.Exists(Path.Combine(sourceRoot, "seed-images.json")))
        {
            sourceRoot = Path.Combine(AppContext.BaseDirectory, "SeedAssets");
        }

        string manifestPath = Path.Combine(sourceRoot, "seed-images.json");
        if (!File.Exists(manifestPath))
        {
            throw new InvalidOperationException(
                "The bundled development seed image manifest is missing. "
                + "Build or publish the API project with its SeedAssets content.");
        }

        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var assets = new List<(string Source, string Target)>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (JsonElement image in manifest.RootElement.GetProperty("images").EnumerateArray())
        {
            string file = image.GetProperty("file").GetString()
                ?? throw new InvalidOperationException("A seed image file path is missing.");
            string[] segments = file.Split('/');
            if (segments.Length != 2
                || (segments[0] != "products" && segments[0] != "categories")
                || segments[1].Contains('\\')
                || segments[1].Contains("..", StringComparison.Ordinal)
                || !segments[1].EndsWith(".jpg", StringComparison.Ordinal)
                || !seen.Add(file))
            {
                throw new InvalidOperationException($"Invalid or duplicate seed image path: {file}");
            }

            string source = Path.Combine(sourceRoot, segments[0], segments[1]);
            if (!File.Exists(source) || new FileInfo(source).Length == 0)
            {
                throw new InvalidOperationException($"The bundled seed image is missing or empty: {file}");
            }

            string target = Path.Combine(uploadsRootPath, segments[0], "seed", segments[1]);
            assets.Add((source, target));
        }

        if (assets.Count == 0)
        {
            throw new InvalidOperationException("The development seed image manifest is empty.");
        }

        foreach ((string source, string target) in assets)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            if (File.Exists(target))
            {
                continue;
            }

            string temporaryPath = target + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.Copy(source, temporaryPath, overwrite: false);
                File.Move(temporaryPath, target, overwrite: false);
            }
            catch (IOException) when (File.Exists(target))
            {
                // A previous migration/startup, or another application instance,
                // already installed the file. Existing local media is preserved.
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }
    }
}
