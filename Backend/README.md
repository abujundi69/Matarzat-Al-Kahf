# Matarzat Al Kahf Backend

Production-oriented ASP.NET Core 9 backend for the bilingual Matarzat Al Kahf B2C e-commerce platform. The solution establishes Clean Architecture boundaries, a SQL Server persistence model, Arabic and English localization, administrative JWT security, and the first catalog-management and browsing workflows.

## Architecture

The solution follows Domain-Driven Design and Clean Architecture:

- `MatarzatAlKahf.Domain` is a Class Library containing entities, relationships, and enums, with no framework or persistence dependencies.
- `MatarzatAlKahf.Application` is a Class Library containing feature-specific contracts, DTOs, validation rules, normalization, pagination, and security constants.
- `MatarzatAlKahf.Infrastructure` is a Class Library containing Entity Framework Core, SQL Server integration, migrations, and the feature service implementations.
- `MatarzatAlKahf.Api` is an ASP.NET Core Web API and the composition root containing HTTP controllers, localization, authentication, authorization, rate limiting, Problem Details, and Swagger.

Project dependencies point inward:

```text
Application    -> Domain
Infrastructure -> Application, Domain
Api            -> Application, Infrastructure
```

The production code contains these four projects: three Class Libraries and one Web API. `MatarzatAlKahf.Api` is the only executable project, and `Frontend` contains the React client. All projects live directly under `Backend`; the solution does not use a `src` directory.

## Implemented features

The current system implements:

- Customer and administrator authentication with short-lived JWT access tokens, OTP registration/password reset, and profile management.
- Idempotent command-line bootstrap of the first `SuperAdmin` plus a separate, Development-only bilingual sample-data seed command.
- Permission-based authorization for catalog, inventory, order, customer, report, content, settings, and system administration. An administrator's permissions are resolved from their stored role-permission mappings, so revoking a mapping takes effect on their next sign-in.
- Revocable administrative sessions: logout, suspension, deletion, and an administrative password reset all invalidate already-issued tokens on the next request rather than leaving access live until the token expires.
- Administrative CRUD for the supported resources, including Arabic/English translations, variants, image metadata, and inventory snapshots.
- Transactional inventory adjustments with non-negative stock checks and row-version concurrency handling.
- Anonymous localized category and product browsing, slug lookup, filtering, sorting, and pagination.
- Anonymous/customer carts, checkout previews, guest phone verification, coupons, order placement, customer order history, and administrative order workflows.
- Payment-method configuration, in-app notifications, content pages, banners, contact messages, imports, reports, audit logs, delivery locations, social links, and SEO/settings APIs.
- Centralized Problem Details responses, login rate limiting, structured logging, and Swagger Bearer authentication.
- One owner for request failures: `ApiExceptionHandler` records every rejected request as a warning carrying the status and trace identifier, and every genuine fault as an error with the exception. The hosting middleware's duplicate error-level entry is therefore filtered off (`Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware: None`), so ordinary 400s from bad input no longer appear as server errors. A client that disconnects mid-request is recorded at debug level instead of as a fault.
- Baseline security response headers (`X-Content-Type-Options`, `Referrer-Policy`, `X-Frame-Options`, and a `default-src 'none'` CSP) on every API response, plus HSTS outside Development. Public upload responses carry only the sniffing and referrer protections so the storefront can embed product imagery cross-origin.

## Requirements

- A .NET SDK capable of targeting `net9.0`.
- SQL Server with the existing migration already applied before using database-backed commands or endpoints.
- The repository-local `dotnet-ef` 9.0.18 tool for migration commands.

The application never applies migrations automatically.

## Restore, build, and format

Run commands from the `Backend` directory:

```powershell
dotnet tool restore
dotnet restore MatarzatAlKahf.sln
dotnet build MatarzatAlKahf.sln --configuration Release
dotnet format MatarzatAlKahf.sln --verify-no-changes
```

## Configuration and running the API

`appsettings.json` deliberately ships an empty `ConnectionStrings:DefaultConnection`, so a deployment that forgets to configure one fails at startup with a clear message instead of silently pointing at a developer machine. Machine-specific connection strings belong in user secrets or the ignored `appsettings.Development.local.json` (copy `appsettings.Development.local.example.json`). Environment variables override these values:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=MatarzatAlKahfDb;Trusted_Connection=True;TrustServerCertificate=True"
```

The API project is initialized for .NET User Secrets. For local Development, store a private JWT signing key of at least 32 bytes outside the repository:

```powershell
dotnet user-secrets set "Jwt:SigningKey" "<strong-random-key-of-at-least-32-bytes>" --project MatarzatAlKahf.Api
dotnet run --project MatarzatAlKahf.Api
```

The committed configuration contains issuer, audience, and token lifetime only; it never contains the signing key. Production can supply the key through `Jwt__SigningKey` or another secure configuration provider. The configured defaults are issuer `MatarzatAlKahf`, audience `MatarzatAlKahf.Admin`, and a 60-minute access-token lifetime. Normal HTTP startup fails clearly if the signing key is missing or too short.

In Development, the default HTTP profile serves Swagger UI at `http://localhost:5212/swagger`; always use the listening URL printed by the API if a port override is supplied. Obtain a token from the unified `POST /api/v1/auth/login` endpoint, select **Authorize**, and enter `Bearer <access-token>`. The response identifies the authenticated actor and returns database-backed roles and permissions for administrative users. Public endpoints do not require authorization; administrative endpoints display and enforce their permission policies.

## Initial administrator bootstrap

The first administrator is created explicitly; there is no hard-coded account or password. The database must already exist and have the checked-in migration applied. Set the required values and run:

```powershell
$env:BootstrapAdmin__FullName = "Store Administrator"
$env:BootstrapAdmin__PhoneNumber = "+970 59 123 4567"
$env:BootstrapAdmin__Email = "admin@example.com" # optional
$env:BootstrapAdmin__Password = "replace-with-a-strong-password"
dotnet run --project MatarzatAlKahf.Api -- --bootstrap-admin
```

The command normalizes the phone number, hashes the password, creates or reuses the system roles and permissions transactionally, assigns the first administrator to `SuperAdmin`, and exits without starting HTTP. It is idempotent and does not run migrations. A second administrator is not silently created when one already exists.

The normalized phone format removes spaces, dashes, parentheses, and other formatting characters, preserves one leading `+`, and does not infer a country code.

## Development database seed

EF Core 9 `UseSeeding` and `UseAsyncSeeding` insert the bilingual Palestinian heritage catalog after migration operations, including when no migrations are pending. No new schema migration is needed. Both callbacks use the same insert-only seeder under EF's migration lock and a transaction-scoped application lock.

From `Backend`, configure the connection once (user secrets or the ignored local JSON file):

```powershell
dotnet tool restore --tool-manifest dotnet-tools.json
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=MatarzatAlKahfDb;Trusted_Connection=True;TrustServerCertificate=True" --project MatarzatAlKahf.Api
dotnet ef database update --project MatarzatAlKahf.Infrastructure --startup-project MatarzatAlKahf.Api
```

In Visual Studio Package Manager Console, select Infrastructure as the default project and API as the startup project:

```powershell
Update-Database -Project MatarzatAlKahf.Infrastructure -StartupProject MatarzatAlKahf.Api
```

The design-time factory defaults to Development when no environment is supplied. Explicit `ASPNETCORE_ENVIRONMENT=Production` or `DOTNET_ENVIRONMENT=Production` disables demo seeding; `DevelopmentSeed__Enabled=false` disables it in Development too. Configure a development/test database, never a live store, before running development tools. Migration scripts contain schema only; EF tooling, migration bundles, or `Database.Migrate/MigrateAsync` execute the callbacks.

The dataset contains 8 categories, 24 active products, Arabic and English translations, priced ILS variants and inventory, local product/category images, nine Palestinian delivery cities, and missing store/payment defaults. It never creates customer accounts, demo orders, or shared administrator passwords. Create the first administrator through the existing `--bootstrap-admin` command.

Existing rows found by stable IDs or natural keys are preserved, including titles, prices, stock/reservations, status, passwords, permissions, shipping fees, and configured settings. Soft-deleted rows are not revived. Repeating migrations or starting the API does not duplicate catalog rows. Normal startup only restores missing bundled image files; it does not migrate or seed the database.

Images are bundled under `MatarzatAlKahf.Api/SeedAssets`, copied without overwriting existing files into the configured `FileStorage:RootPath`, and stored in the database as `/uploads/products/seed/...jpg` and `/uploads/categories/seed/...jpg`. They work offline after checkout and publish. Source URLs, authors, licenses and representative-image notes are recorded with the assets.

An optional explicit rerun is still available:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project MatarzatAlKahf.Api --no-launch-profile -- --seed-development
```

Run the isolated SQL Server integration verifier (three generated LocalDB databases, automatically removed):

```powershell
dotnet run --project scripts/SeedVerification --configuration Release
```

It checks both migration callbacks, repeat and concurrent runs, complete row preservation after merchant edits, account hashes/roles, stock reservations, soft deletion, relationships, local assets, and Production/disabled-seed behavior. See `scripts/SeedVerification/README.md`.
Do not run the legacy `--purge-demo-data` maintenance command against a populated store: it deletes business data. Use a separate production database and configure real shipping/payment values for deployment.

## Localization

Arabic (`ar`) and English (`en`) are supported, with Arabic as the default request culture. Use the standard `Accept-Language` request header:

```http
GET /api/v1/products
Accept-Language: ar
```

```http
GET /api/v1/products
Accept-Language: en
```

Public catalog responses include only the selected translation and return `Content-Language`. When a selected translation is missing in legacy data, the service falls back to Arabic and then English. Administrative responses include both translations.

## API surface

Anonymous authentication and catalog endpoints:

```text
POST /api/v1/auth/login
GET /api/v1/categories
GET /api/v1/categories/{id}
GET /api/v1/categories/by-slug/{slug}
GET /api/v1/products
GET /api/v1/products/{id}
GET /api/v1/products/by-slug/{slug}
```

The product list accepts `page`, `pageSize`, `search`, `categoryId`, `minPrice`, `maxPrice`, `isFeatured`, `inStock`, and `sort`. Valid sort values are `newest`, `price-asc`, `price-desc`, and `name`.

Authenticated session endpoints:

```text
POST /api/v1/admin/auth/logout
POST /api/v1/customer/auth/logout
```

Each revokes the session behind the presented token, so the token is refused from the next request
onward instead of remaining valid until it expires.

Protected administration endpoints:

```text
GET    /api/v1/admin/categories
GET    /api/v1/admin/categories/{id}
POST   /api/v1/admin/categories
PUT    /api/v1/admin/categories/{id}
DELETE /api/v1/admin/categories/{id}
GET    /api/v1/admin/products
GET    /api/v1/admin/products/{id}
POST   /api/v1/admin/products
PUT    /api/v1/admin/products/{id}
DELETE /api/v1/admin/products/{id}
GET    /api/v1/admin/inventory
GET    /api/v1/admin/inventory/low-stock
GET    /api/v1/admin/inventory/{productVariantId}
POST   /api/v1/admin/inventory/{productVariantId}/adjust
```

Administrative category, product, inventory, and low-stock list endpoints accept
`page` and `pageSize` query parameters. Defaults are 1 and 20, and the maximum
page size is 100. Their response includes `items`, `page`, `pageSize`,
`totalItems`, and `totalPages`.

MARS is intentionally disabled in the default connection string because the
current request workflows do not require it and EF Core transaction savepoints
must remain available.

The login endpoint is anonymous but limited to five attempts per minute per client IP. All other administrative endpoints require a JWT containing the appropriate permission claim.

## Entity Framework Core migrations

Create future migrations with Infrastructure as the migration project and API as the startup project:

```powershell
dotnet ef migrations add MigrationName --project MatarzatAlKahf.Infrastructure --startup-project MatarzatAlKahf.Api --output-dir Persistence/Migrations
```

Check model drift without modifying a database:

```powershell
dotnet ef migrations has-pending-model-changes --project MatarzatAlKahf.Infrastructure --startup-project MatarzatAlKahf.Api --configuration Release
```

The original `InitialCreate` migration is preserved. Phase One uses the existing schema and does not create an empty migration. Run `dotnet ef database update` only as an explicit deployment or operator action; API startup and admin bootstrap never do so.

## Full API setup

The production API now includes administration, customer accounts, cart,
checkout, orders, offline payments, content, notifications, reports, Excel
import/export, SEO, audit, files, and delivery rules in addition to the original
catalog.

From `Backend`, restore tools and packages:

```powershell
dotnet tool restore
dotnet restore MatarzatAlKahf.sln
```

Store development secrets outside source control:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=MatarzatAlKahfDb;Trusted_Connection=True;TrustServerCertificate=True" --project MatarzatAlKahf.Api
dotnet user-secrets set "Jwt:SigningKey" "<at-least-32-random-UTF8-bytes>" --project MatarzatAlKahf.Api
dotnet user-secrets set "BootstrapAdmin:FullName" "Store Administrator" --project MatarzatAlKahf.Api
dotnet user-secrets set "BootstrapAdmin:PhoneNumber" "+966..." --project MatarzatAlKahf.Api
dotnet user-secrets set "BootstrapAdmin:Email" "admin@example.com" --project MatarzatAlKahf.Api
dotnet user-secrets set "BootstrapAdmin:Password" "<strong-password>" --project MatarzatAlKahf.Api
dotnet user-secrets set "FileStorage:RootPath" "C:\MatarzatAlKahf\files" --project MatarzatAlKahf.Api
```

Apply migrations, bootstrap the idempotent SuperAdmin, and run:

```powershell
dotnet ef database update --project MatarzatAlKahf.Infrastructure --startup-project MatarzatAlKahf.Api
dotnet run --project MatarzatAlKahf.Api -- --bootstrap-admin
dotnet run --project MatarzatAlKahf.Api
```

Swagger is available in Development at `/swagger`; liveness and database
readiness are `/health/live` and `/health/ready`.

`FileStorage:RootPath` must be a writable deployment-owned directory. Files are
validated by size, extension, declared media type, and signature. Private files
remain authorization-protected.

OTP workflows are complete, but production deliberately registers a disabled
delivery adapter until an `IOtpDeliveryService` provider is supplied. The
capture-file provider is available only when both the Development environment
and `Otp:EnableTestProvider=true` are set (it writes the code to the absolute path
in `Otp:TestCapturePath`); it is intended solely for local development. Twilio is
not installed.

The development seed enables cash on delivery; bank transfer, wallet, and card methods require operator configuration. Manual wallet
transfer can be enabled through the payment-method administration API. Card
payments remain unavailable until a verified `IOnlinePaymentGateway` is
configured; the API never fabricates a successful online payment.

For deployment, provide a production SQL Server connection string, a rotated
JWT signing key, an absolute writable file root, explicit CORS origins, HTTPS
termination, and a real OTP provider. Apply migrations as a deployment step;
the API does not silently migrate its database at startup.
