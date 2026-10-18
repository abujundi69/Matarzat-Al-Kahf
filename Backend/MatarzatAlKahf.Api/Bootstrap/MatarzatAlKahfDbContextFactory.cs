using MatarzatAlKahf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using MatarzatAlKahf.Infrastructure.Persistence.Development;

namespace MatarzatAlKahf.Api.Bootstrap;

/// <summary>Migrations do not require JWT, SMTP, or a running web server.</summary>
public sealed class MatarzatAlKahfDbContextFactory
    : IDesignTimeDbContextFactory<MatarzatAlKahfDbContext>
{
    public MatarzatAlKahfDbContext CreateDbContext(string[] args)
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        string contentRoot = new[]
        {
            currentDirectory,
            Path.Combine(currentDirectory, "MatarzatAlKahf.Api"),
            Path.Combine(currentDirectory, "Backend", "MatarzatAlKahf.Api"),
            Path.GetDirectoryName(typeof(MatarzatAlKahfDbContextFactory).Assembly.Location)!
        }.FirstOrDefault(path => File.Exists(Path.Combine(path, "appsettings.json")))
            ?? throw new InvalidOperationException(
                "API appsettings.json was not found. Run EF tools from Backend or the API directory.");

        // EF tools default to Development for the local Update-Database workflow.
        // Explicit Production/Staging environments never install the demo seed callbacks.
        IConfiguration commandLine = new ConfigurationBuilder().AddCommandLine(args).Build();
        string environment = commandLine["environment"]
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environments.Development;
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ApplicationName = typeof(MatarzatAlKahfDbContextFactory).Assembly.GetName().Name,
            ContentRootPath = contentRoot,
            EnvironmentName = environment
        });
        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddJsonFile(
                "appsettings.Development.local.json", optional: true, reloadOnChange: false);
            builder.Configuration.AddUserSecrets<MatarzatAlKahfDbContextFactory>(optional: true);
        }
        builder.Configuration.AddEnvironmentVariables();
        builder.Configuration.AddCommandLine(args);
        var options = new DbContextOptionsBuilder<MatarzatAlKahfDbContext>();
        string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Configure ConnectionStrings:DefaultConnection in user secrets, environment "
                + "variables, or appsettings.Development.local.json before updating the database.");
        }

        options.UseSqlServer(connectionString, sql =>
        {
            sql.MigrationsAssembly("MatarzatAlKahf.Infrastructure");
            sql.EnableRetryOnFailure();
            sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });
        options.ConfigureDevelopmentSeed(builder.Configuration, builder.Environment);
        return new MatarzatAlKahfDbContext(options.Options);
    }
}
