# Matarzat Al Kahf

Bilingual (Arabic/English) B2C e-commerce platform for a Palestinian embroidery and heritage-gift
store: an ASP.NET Core 9 API with a React 19 storefront and administration console.

## Layout

| Path | Contents |
| --- | --- |
| `Backend/MatarzatAlKahf.Api` | ASP.NET Core Web API — the only executable project |
| `Backend/MatarzatAlKahf.Application` | Contracts, DTOs, validation, security constants |
| `Backend/MatarzatAlKahf.Domain` | Entities and enums, with no framework dependencies |
| `Backend/MatarzatAlKahf.Infrastructure` | EF Core persistence, migrations, feature services |
| `Backend/Frontend` | React + Vite client (storefront, customer area, admin console) |
| `Matarzat_Al_Kahf_Project_Requirements_EN.md` | Product requirements |

`Backend/README.md` is the operational reference: configuration, secrets, migrations, and the
maintenance commands. `Backend/Frontend/README.md` covers the client.

## Run both halves locally

Requires a .NET SDK able to target `net9.0`, Node.js 20+, and SQL Server (LocalDB is fine).

```powershell
# One-time: store the secrets the API refuses to start without
dotnet user-secrets set "Jwt:SigningKey" "<at-least-32-random-UTF8-bytes>" --project Backend/MatarzatAlKahf.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=MatarzatAlKahfDb;Trusted_Connection=True;TrustServerCertificate=True" --project Backend/MatarzatAlKahf.Api

# Apply migrations, start the API on :5212 and the client on :5173
.\start-dev.ps1

# Optional idempotent rerun (migrations already insert the Development dataset)
.\start-dev.ps1 -Seed
```

The client proxies `/api`, `/uploads`, `/robots.txt`, and `/sitemap.xml` to the API, so open
`http://127.0.0.1:5173` and sign in there. Swagger is at `http://127.0.0.1:5212/swagger`.

## First administrator

There is no built-in account and no password in source control. Create the first `SuperAdmin`
explicitly after the migrations are applied:

```powershell
$env:BootstrapAdmin__FullName = "Store Owner"
$env:BootstrapAdmin__PhoneNumber = "+970..."
$env:BootstrapAdmin__Email = "owner@example.com"   # optional
$env:BootstrapAdmin__Password = "<strong-password>"
dotnet run --project Backend/MatarzatAlKahf.Api -- --bootstrap-admin
```

## Development data

Database updates automatically insert 8 heritage categories, 24 bilingual products, local licensed images, inventory, Palestinian delivery cities, and missing settings. Existing merchant data and accounts are preserved. No shared demo passwords or orders are created. Set an explicit Production environment to disable development seed callbacks. See Backend/README.md for setup, image attribution and isolated verification.

## Build

```powershell
dotnet build Backend/MatarzatAlKahf.sln --configuration Release

cd Backend/Frontend
npm ci
npm run typecheck
npm run lint
npm run build
```
