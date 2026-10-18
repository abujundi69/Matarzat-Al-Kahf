using MatarzatAlKahf.Infrastructure.Features.Files.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace MatarzatAlKahf.Api.Uploads;

public static class PublicUploadsApplicationBuilderExtensions
{
    public static WebApplication UsePublicUploadFiles(
        this WebApplication app,
        IConfiguration configuration)
    {
        string rootPath = FileStorageOptions.ResolveRootPath(
            configuration[$"{FileStorageOptions.SectionName}:RootPath"],
            app.Environment.ContentRootPath);
        Directory.CreateDirectory(rootPath);

        var contentTypes = new FileExtensionContentTypeProvider();
        contentTypes.Mappings.Clear();
        contentTypes.Mappings[".jpg"] = "image/jpeg";
        contentTypes.Mappings[".jpeg"] = "image/jpeg";
        contentTypes.Mappings[".png"] = "image/png";
        contentTypes.Mappings[".webp"] = "image/webp";

        foreach (string folder in FileStorageOptions.PublicFolders)
        {
            string folderPath = Path.Combine(rootPath, folder);
            Directory.CreateDirectory(folderPath);
            var provider = new PhysicalFileProvider(folderPath);
            app.Lifetime.ApplicationStopped.Register(provider.Dispose);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = provider,
                RequestPath = $"/uploads/{folder}",
                ContentTypeProvider = contentTypes,
                ServeUnknownFileTypes = false,
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers.CacheControl =
                        "public,max-age=31536000,immutable";
                    context.Context.Response.Headers.XContentTypeOptions = "nosniff";
                }
            });
        }

        return app;
    }
}
