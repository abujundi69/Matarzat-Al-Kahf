using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MatarzatAlKahf.Api.Bootstrap;
using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Entities.Identity;
using MatarzatAlKahf.Domain.Enums;
using MatarzatAlKahf.Infrastructure.Authentication.Admin;
using MatarzatAlKahf.Infrastructure.Persistence;
using MatarzatAlKahf.Infrastructure.Persistence.Development;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

// This executable deliberately accepts no server/database/connection-string arguments.
// All data mutations and teardown are restricted to new databases created by this run.
const string databasePrefix = "MatarzatSeedVerification_";
bool keepDatabases = args.Contains("--keep-databases", StringComparer.Ordinal);
if (args.Any(argument => argument != "--keep-databases"))
{
    Console.Error.WriteLine("Usage: dotnet run --project scripts/SeedVerification [-- --keep-databases]");
    return 2;
}

string backendRoot = FindBackendRoot();
Directory.SetCurrentDirectory(backendRoot);
string runId = Guid.NewGuid().ToString("N");
string uploadsRoot = Path.Combine(Path.GetTempPath(), databasePrefix + runId, "uploads");
var databases = new HashSet<string>(StringComparer.Ordinal);
var environmentBackup = new Dictionary<string, string?>();
int checks = 0;
int exitCode = 0;

try
{
    SetEnvironment("DOTNET_ENVIRONMENT", "Development");
    SetEnvironment("ASPNETCORE_ENVIRONMENT", "Development");
    SetEnvironment("DevelopmentSeed__Enabled", "true");
    SetEnvironment("FileStorage__RootPath", uploadsRoot);

    string syncDatabase = databasePrefix + runId + "_sync";
    await EnsureNewDatabaseAsync(syncDatabase);
    await using (var context = CreateContext(syncDatabase))
    {
        // Deliberately use the synchronous path used by PMC Update-Database.
        context.Database.Migrate();
        await AssertCleanSeedAsync(context);
    }
    Console.WriteLine("PASS: clean synchronous migration callback (Update-Database path).");

    Dictionary<string, string> original = await SnapshotAsync(syncDatabase);
    await using (var context = CreateContext(syncDatabase)) context.Database.Migrate();
    await AssertSnapshotAsync(syncDatabase, original, "unchanged synchronous migration");

    await MutateMerchantDataAsync(syncDatabase);
    Dictionary<string, string> merchantSnapshot = await SnapshotAsync(syncDatabase);

    await using (var context = CreateContext(syncDatabase)) await context.Database.MigrateAsync();
    await AssertSnapshotAsync(syncDatabase, merchantSnapshot, "asynchronous migration preserves merchant edits");

    await using (var context = CreateContext(syncDatabase))
    {
        DevelopmentSeedResult result = await new DevelopmentDataSeeder(context).SeedAsync(default);
        AssertSeedSummary(result);
        AssertSeedSummary(await new DevelopmentDataSeeder(context).SeedAsync(default));
    }
    await AssertSnapshotAsync(syncDatabase, merchantSnapshot, "repeated explicit seeding preserves every stored row");

    // Different contexts/connections exercise EF's migration lock and the explicit seed lock.
    await Task.WhenAll(MigrateAsync(syncDatabase), MigrateAsync(syncDatabase));
    await AssertSnapshotAsync(syncDatabase, merchantSnapshot, "concurrent migrations are idempotent");
    await Task.WhenAll(SeedAsync(syncDatabase), SeedAsync(syncDatabase), MigrateAsync(syncDatabase));
    await AssertSnapshotAsync(syncDatabase, merchantSnapshot, "concurrent explicit seed and migration are idempotent");

    string asyncDatabase = databasePrefix + runId + "_async";
    await EnsureNewDatabaseAsync(asyncDatabase);
    await using (var context = CreateContext(asyncDatabase))
    {
        await context.Database.MigrateAsync();
        await AssertCleanSeedAsync(context);
    }
    Console.WriteLine("PASS: clean asynchronous migration callback.");

    string productionDatabase = databasePrefix + runId + "_production";
    SetEnvironment("DOTNET_ENVIRONMENT", "Production");
    SetEnvironment("ASPNETCORE_ENVIRONMENT", "Production");
    await EnsureNewDatabaseAsync(productionDatabase);
    await using (var context = CreateContext(productionDatabase))
    {
        await context.Database.MigrateAsync();
        Check(!await context.Categories.AnyAsync() && !await context.Products.AnyAsync(),
            "Production must not install demo categories or products, even with the seed flag true.");
        Check(!(await context.Database.GetPendingMigrationsAsync()).Any(), "Production migrations must complete.");
    }
    Console.WriteLine("PASS: Production migrations do not seed the demo catalogue.");
    SetEnvironment("DOTNET_ENVIRONMENT", "Development");
    SetEnvironment("ASPNETCORE_ENVIRONMENT", "Development");
    await using (var context = CreateContext(productionDatabase, ["--environment", "Production"]))
    {
        AssertCallbacks(context, expected: false);
        await context.Database.MigrateAsync();
        Check(!await context.Categories.AnyAsync() && !await context.Products.AnyAsync(),
            "A forwarded Production argument must override Development environment variables.");
    }
    await using (var context = CreateContext(productionDatabase, ["--DevelopmentSeed:Enabled", "false"]))
    {
        AssertCallbacks(context, expected: false);
        context.Database.Migrate();
        Check(!await context.Categories.AnyAsync() && !await context.Products.AnyAsync(),
            "DevelopmentSeed:Enabled=false must prevent catalogue seeding.");
    }
    await using (var context = CreateContext(productionDatabase)) AssertCallbacks(context, expected: true);
    Console.WriteLine("PASS: forwarded Production argument and disabled Development seed flag suppress both callbacks.");
    Console.WriteLine($"SUCCESS: {checks} assertions passed across three isolated LocalDB databases.");
}
catch (Exception exception)
{
    exitCode = 1;
    Console.Error.WriteLine($"FAIL: {exception}");
}
finally
{
    if (keepDatabases)
    {
        foreach (string database in databases) Console.WriteLine($"Retained test database: {database}");
        Console.WriteLine($"Retained test uploads: {uploadsRoot}");
    }
    else
    {
        foreach (string database in databases)
        {
            try
            {
                await using var context = CreateContext(database);
                var parsed = new SqlConnectionStringBuilder(context.Database.GetConnectionString());
                if (!database.StartsWith(databasePrefix, StringComparison.Ordinal)
                    || !databases.Contains(database)
                    || parsed.InitialCatalog != database
                    || !parsed.DataSource.Equals(@"(localdb)\MSSQLLocalDB", StringComparison.OrdinalIgnoreCase))
                {
                    exitCode = 1;
                    Console.Error.WriteLine("Refusing unsafe database teardown.");
                    continue;
                }
                await context.Database.EnsureDeletedAsync();
                Console.WriteLine($"Removed test database: {database}");
            }
            catch (Exception exception)
            {
                exitCode = 1;
                Console.Error.WriteLine($"Test database cleanup failed: {database}: {exception.Message}");
            }
        }

        string temporaryRoot = Path.GetFullPath(Path.GetTempPath());
        string generatedRoot = Path.GetFullPath(Path.GetDirectoryName(uploadsRoot)!);
        if (generatedRoot.StartsWith(temporaryRoot, StringComparison.OrdinalIgnoreCase)
            && Path.GetFileName(generatedRoot) == databasePrefix + runId
            && Directory.Exists(generatedRoot))
        {
            Directory.Delete(generatedRoot, recursive: true);
        }
    }
    foreach ((string key, string? value) in environmentBackup)
        Environment.SetEnvironmentVariable(key, value);
}

return exitCode;

void SetEnvironment(string key, string value)
{
    environmentBackup.TryAdd(key, Environment.GetEnvironmentVariable(key));
    Environment.SetEnvironmentVariable(key, value);
}

MatarzatAlKahfDbContext CreateContext(string database, string[]? factoryArguments = null)
{
    if (!database.StartsWith(databasePrefix, StringComparison.Ordinal)
        || !Regex.IsMatch(database, @"^MatarzatSeedVerification_[a-f0-9]{32}_(sync|async|production)$"))
        throw new InvalidOperationException("Refusing a database outside this verification workflow.");

    var connection = new SqlConnectionStringBuilder
    {
        DataSource = @"(localdb)\MSSQLLocalDB",
        InitialCatalog = database,
        IntegratedSecurity = true,
        TrustServerCertificate = true,
        ConnectTimeout = 15
    };
    SetEnvironment("ConnectionStrings__DefaultConnection", connection.ConnectionString);
    return new MatarzatAlKahfDbContextFactory().CreateDbContext(factoryArguments ?? []);
}

async Task EnsureNewDatabaseAsync(string database)
{
    await using var context = CreateContext(database);
    var connectionString = new SqlConnectionStringBuilder(context.Database.GetConnectionString()) { InitialCatalog = "master" };
    await using var connection = new SqlConnection(connectionString.ConnectionString);
    await connection.OpenAsync();
    await using var command = connection.CreateCommand();
    command.CommandText = "SELECT database_id FROM sys.databases WHERE name = @name";
    command.Parameters.AddWithValue("@name", database);
    if (await command.ExecuteScalarAsync() is not null)
        throw new InvalidOperationException($"Refusing to reuse an existing database: {database}");
    databases.Add(database);
    Console.WriteLine($"Using new isolated test database: {database}");
}

void Check(bool condition, string message)
{
    Interlocked.Increment(ref checks);
    if (!condition) throw new InvalidOperationException(message);
}

void AssertCallbacks(MatarzatAlKahfDbContext context, bool expected)
{
    var options = context.GetService<IDbContextOptions>().FindExtension<CoreOptionsExtension>();
    Check((options?.Seeder is not null) == expected && (options?.AsyncSeeder is not null) == expected,
        "Synchronous and asynchronous seed callbacks do not respect the environment/seed flag.");
}

async Task AssertCleanSeedAsync(MatarzatAlKahfDbContext context)
{
    Check(!(await context.Database.GetPendingMigrationsAsync()).Any(), "All migrations must be applied.");
    var categories = await context.Categories.AsNoTracking().Include(item => item.Translations).ToListAsync();
    var products = await context.Products.AsNoTracking()
        .Include(item => item.Translations)
        .Include(item => item.Images).ThenInclude(item => item.Translations)
        .Include(item => item.Variants).ThenInclude(item => item.InventoryItem)
        .ToListAsync();
    Check(categories.Count == 8, $"Expected 8 categories; found {categories.Count}.");
    Check(products.Count == 24, $"Expected 24 products; found {products.Count}.");
    Check(products.Sum(item => item.Images.Count) == 24, "Expected one photo for each of 24 products.");
    Check(await context.DeliveryLocations.CountAsync() >= 9, "Expected at least nine Palestinian delivery locations.");
    Check(await context.StoreSettings.AnyAsync(), "Required store defaults must be seeded.");
    Check(!await context.AdminUsers.AnyAsync() && !await context.Customers.AnyAsync(),
        "Demo seeding must not create accounts or known passwords.");
    Check(!await context.Orders.AnyAsync(), "The catalogue seed must not create demo orders.");
    Check(products.Any(item => item.IsFeatured), "The homepage needs featured products.");

    var categoryIds = categories.Select(item => item.Id).ToHashSet();
    var skus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var localImages = new HashSet<string>(StringComparer.Ordinal);
    foreach (var category in categories)
    {
        Check(FrontendGuid(category.Id), "Category IDs must be accepted by the existing frontend.");
        Check(category.IsActive && !category.IsDeleted, "Initial seed categories must be active.");
        Check(category.Translations.Select(item => item.Language).ToHashSet().SetEquals(Enum.GetValues<LanguageCode>()),
            "Every category needs exactly Arabic and English translations.");
        foreach (var translation in category.Translations)
            CheckContent(translation.Language, translation.Name, translation.Slug, translation.Description);
        CheckImage(category.ImageUrl, "categories");
    }
    foreach (var product in products)
    {
        Check(FrontendGuid(product.Id), "Product IDs must be accepted by the existing frontend.");
        Check(categoryIds.Contains(product.CategoryId), "A product category foreign key is invalid.");
        Check(product.BasePrice > 0 && product.Currency == CurrencyCode.ILS, "Product prices must be positive ILS amounts.");
        Check(product.Status == ProductStatus.Active && product.PublishedAtUtc.HasValue, "Seed products must be published.");
        Check(product.Translations.Select(item => item.Language).ToHashSet().SetEquals(Enum.GetValues<LanguageCode>()),
            "Every product needs exactly Arabic and English translations.");
        foreach (var translation in product.Translations)
            CheckContent(translation.Language, translation.Name, translation.Slug, translation.Description);
        Check(product.Images.Count(item => item.IsPrimary) == 1, "Each product needs exactly one primary image.");
        foreach (var image in product.Images)
        {
            Check(image.ProductId == product.Id, "A product image foreign key is invalid.");
            CheckImage(image.ImageUrl, "products");
            CheckImage(image.ThumbnailUrl, "products");
            Check(image.Translations.Select(item => item.Language).ToHashSet().SetEquals(Enum.GetValues<LanguageCode>()),
                "Every product image needs Arabic and English alt text.");
            Check(image.Translations.All(item => item.ProductImageId == image.Id && !string.IsNullOrWhiteSpace(item.AltText)),
                "Product image translations must have valid foreign keys and alt text.");
        }
        Check(product.Variants.Count > 0, "Every product needs a purchasable variant.");
        foreach (var variant in product.Variants)
        {
            Check(variant.ProductId == product.Id && variant.IsActive, "A variant is inactive or has an invalid product foreign key.");
            Check(!string.IsNullOrWhiteSpace(variant.Sku) && skus.Add(variant.Sku), "Variant SKUs must be nonempty and unique.");
            Check(variant.InventoryItem is not null && variant.InventoryItem.ProductVariantId == variant.Id,
                "Every variant needs its own valid inventory row.");
            Check(variant.InventoryItem!.QuantityOnHand > 0 && variant.InventoryItem.ReservedQuantity == 0,
                "Fresh demo inventory must be available without fake reservations.");
        }
    }
    Check(localImages.Count == 32, $"Expected 32 distinct category/product photos; found {localImages.Count}.");
    foreach (var location in await context.DeliveryLocations.AsNoTracking().ToListAsync())
        Check(location.CountryCode == "PS" && !string.IsNullOrWhiteSpace(location.NameArabic)
            && !string.IsNullOrWhiteSpace(location.NameEnglish), "Delivery locations must be bilingual Palestinian cities.");

    await context.Database.OpenConnectionAsync();
    await using var command = context.Database.GetDbConnection().CreateCommand();
    command.CommandText = "DBCC CHECKCONSTRAINTS WITH ALL_CONSTRAINTS, NO_INFOMSGS";
    await using var reader = await command.ExecuteReaderAsync();
    Check(!await reader.ReadAsync(), "SQL Server found a violated database constraint.");
    Console.WriteLine($"PASS: {categories.Count} categories, {products.Count} products, {skus.Count} stocked variants, 32 local images, bilingual content and valid foreign keys.");

    void CheckContent(LanguageCode language, string name, string slug, string description)
    {
        Check(!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(slug)
            && !string.IsNullOrWhiteSpace(description), "Localized catalogue content must be complete.");
        Check(!Regex.IsMatch(name + description, "Test Product|Lorem Ipsum|Sample Product|[\uFFFD\u00C3\u00D8\u00D9]", RegexOptions.IgnoreCase),
            "Catalogue content contains placeholders or broken character encoding.");
        if (language == LanguageCode.Arabic)
            Check(Regex.IsMatch(name, "[\u0600-\u06FF]"), "Arabic names must contain Arabic text.");
    }

    void CheckImage(string url, string folder)
    {
        string prefix = $"/uploads/{folder}/seed/";
        Check(url.StartsWith(prefix, StringComparison.Ordinal) && url.EndsWith(".jpg", StringComparison.Ordinal),
            "Seed image URLs must use the existing local uploads architecture.");
        string filename = url[prefix.Length..];
        Check(Path.GetFileName(filename) == filename && !filename.Contains("..", StringComparison.Ordinal),
            "A seed image URL contains unsafe path segments.");
        string file = Path.Combine(uploadsRoot, folder, "seed", filename);
        Check(File.Exists(file) && new FileInfo(file).Length > 1000, "A seeded image file is missing or empty.");
        byte[] header = new byte[3];
        using var stream = File.OpenRead(file);
        Check(stream.Read(header) == 3 && header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff,
            "A .jpg seed image does not have a JPEG signature.");
        localImages.Add(url);
    }
}

async Task MutateMerchantDataAsync(string database)
{
    await using var context = CreateContext(database);
    var title = await context.ProductTranslations.FirstAsync(item => item.Language == LanguageCode.Arabic);
    title.Name = "اسم عدله صاحب المتجر";
    title.UpdatedAtUtc = DateTime.UtcNow;
    var inventory = await context.InventoryItems.FirstAsync();
    inventory.QuantityOnHand = 73;
    inventory.ReservedQuantity = 7;
    var category = await context.Categories.FirstAsync();
    category.IsActive = false;
    var hiddenProduct = await context.Products.FirstAsync(item => item.Id != title.ProductId);
    hiddenProduct.IsDeleted = true;
    var setting = await context.StoreSettings.FirstAsync();
    setting.Value = "Merchant-controlled value";
    var location = await context.DeliveryLocations.FirstAsync();
    location.AdditionalDeliveryCost = 37.50m;
    location.CashOnDeliveryAllowed = false;

    string suffix = Guid.NewGuid().ToString("N");
    var merchantCategory = new Category
    {
        IsActive = true,
        ImageUrl = "/uploads/categories/merchant-owned.jpg",
        SortOrder = 100,
        Translations =
        [
            new CategoryTranslation { Language = LanguageCode.Arabic, Name = "قسم التاجر الخاص", Slug = "merchant-ar-" + suffix, Description = "بيانات مستقلة أنشأها صاحب المتجر" },
            new CategoryTranslation { Language = LanguageCode.English, Name = "Merchant collection", Slug = "merchant-en-" + suffix, Description = "Independent merchant catalogue data" }
        ]
    };
    context.Categories.Add(merchantCategory);
    var merchantProduct = new Product
    {
        Category = merchantCategory,
        BasePrice = 199m,
        Currency = CurrencyCode.ILS,
        Status = ProductStatus.Draft,
        Translations =
        [
            new ProductTranslation { Language = LanguageCode.Arabic, Name = "قطعة التاجر الخاصة", Slug = "merchant-product-ar-" + suffix, Description = "منتج مستقل لصاحب المتجر" },
            new ProductTranslation { Language = LanguageCode.English, Name = "Merchant handmade piece", Slug = "merchant-product-en-" + suffix, Description = "An independent merchant product" }
        ]
    };
    context.Products.Add(merchantProduct);
    var merchantVariant = new ProductVariant
    {
        Product = merchantProduct,
        Sku = "MERCHANT-" + suffix,
        IsActive = true,
        InventoryItem = new InventoryItem { QuantityOnHand = 11, ReservedQuantity = 2, ReorderLevel = 3 }
    };
    context.ProductVariants.Add(merchantVariant);

    var admin = new AdminUser
    {
        FullName = "Verification merchant",
        PhoneNumber = "+970599000001",
        NormalizedPhoneNumber = "+970599000001",
        Status = UserStatus.Active,
        PreferredLanguage = LanguageCode.Arabic
    };
    string password = "A!" + Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
    var hasher = new AdminPasswordHasher();
    admin.PasswordHash = hasher.HashPassword(admin, password);
    Check(admin.PasswordHash != password && hasher.VerifyPassword(admin, password),
        "The existing application password hasher must create a valid nonplaintext hash.");
    admin.UserRoles.Add(new AdminUserRole
    {
        AdminUser = admin,
        Role = new Role { Name = "VerificationMerchantRole", Description = "Owned by the isolated verifier" }
    });
    context.AdminUsers.Add(admin);
    await context.SaveChangesAsync();
    Console.WriteLine("Prepared merchant title, stock/reservations, inactive category, soft-deleted product, settings/shipping edits, independent catalogue and hashed admin/role.");
}

async Task MigrateAsync(string database)
{
    await using var context = CreateContext(database);
    await context.Database.MigrateAsync();
}

async Task SeedAsync(string database)
{
    await using var context = CreateContext(database);
    AssertSeedSummary(await new DevelopmentDataSeeder(context).SeedAsync(default));
}

void AssertSeedSummary(DevelopmentSeedResult result)
{
    // The seed result reports matching catalogue totals, rather than insert counts.
    // Full-table snapshots below detect any actual inserted, updated or deleted row.
    Check(result == new DevelopmentSeedResult(0, 0, 0, 9, 8, 24, 30, 0, 0, 0, 2),
        "The repeated seed summary changed or reports unexpected account/order data.");
}

async Task<Dictionary<string, string>> SnapshotAsync(string database)
{
    await using var context = CreateContext(database);
    await context.Database.OpenConnectionAsync();
    DbConnection connection = context.Database.GetDbConnection();
    var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
    foreach (var table in context.Model.GetRelationalModel().Tables.OrderBy(table => table.Schema).ThenBy(table => table.Name))
    {
        string qualifiedName = Quote(table.Schema ?? "dbo") + "." + Quote(table.Name);
        string order = string.Join(",", table.PrimaryKey?.Columns.Select(column => Quote(column.Name)) ?? []);
        if (string.IsNullOrEmpty(order)) throw new InvalidOperationException("Cannot snapshot a table without a primary key.");
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {qualifiedName} ORDER BY {order} FOR JSON PATH, INCLUDE_NULL_VALUES";
        await using var reader = await command.ExecuteReaderAsync();
        var json = new StringBuilder();
        while (await reader.ReadAsync()) json.Append(reader.GetString(0));
        snapshots[qualifiedName] = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json.ToString())));
    }
    return snapshots;
}

async Task AssertSnapshotAsync(string database, Dictionary<string, string> expected, string stage)
{
    Dictionary<string, string> actual = await SnapshotAsync(database);
    Check(expected.Count == actual.Count, $"Database tables changed during {stage}.");
    foreach ((string table, string fingerprint) in expected)
        Check(actual.GetValueOrDefault(table) == fingerprint, $"Stored rows changed in {table} during {stage}.");
    Console.WriteLine($"PASS: {stage}; all {expected.Count} mapped tables match byte-for-byte snapshots.");
}

static string Quote(string identifier) => "[" + identifier.Replace("]", "]]", StringComparison.Ordinal) + "]";

static bool FrontendGuid(Guid id) => Regex.IsMatch(id.ToString(),
    "^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$");

static string FindBackendRoot()
{
    foreach (string startingPoint in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
    {
        for (var directory = new DirectoryInfo(startingPoint); directory is not null; directory = directory.Parent)
            if (Directory.Exists(Path.Combine(directory.FullName, "MatarzatAlKahf.Api"))) return directory.FullName;
    }
    throw new DirectoryNotFoundException("Could not find the Backend directory containing MatarzatAlKahf.Api.");
}
