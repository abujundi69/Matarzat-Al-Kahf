# Development seed verification

Verified on 2026-10-18 with .NET SDK 10.0.300 targeting .NET 9 and EF Core/SQL Server 9.0.18.

- `dotnet clean`, restore, Debug build, and Release build passed with zero warnings/errors.
- Clean `dotnet ef database update` applied all ten existing migrations, inserted 8 categories, 24 products, 30 variants/inventory rows and 24 product-image records. A repeated update preserved the data. SQL Server `DBCC CHECKCONSTRAINTS` found no violations; EF reported no pending model changes.
- `dotnet run --project scripts/SeedVerification --configuration Release` passed 1,818 assertions across three unique LocalDB databases. Coverage includes clean sync/async callbacks, repeat/concurrent migration and explicit seed runs, full-row preservation after merchant edits, soft deletion, account hashes/roles, inventory reservations, foreign keys, Arabic/English content, local images, Production exclusion, forwarded environment arguments and disabled-seed configuration. Generated databases/uploads were removed.
- The configured development database was seeded additively. Its existing administrator and seven original settings (including their translations) remained unchanged. No demo accounts or orders were added.
- Frontend production build and lint passed. Actual existing React components rendered with live API data in Arabic and English passed 918 assertions covering homepage, categories, category details/filtered products, product listing, all 24 product details and admin products/categories. Temporary component-verification scripts were removed.
- All 32 local image URLs returned HTTP 200 with `image/jpeg` and valid JPEG bytes. All bundled photographs decoded successfully and matched recorded SHA-256 hashes. They represent 14 licensed source photographs; image credits and representative-photo notices are included.
- Release publishing included all 32 JPEGs, the manifest and credits, and excluded the machine-specific local configuration.
- Gitleaks reported no leaks in the publication source. Real environment files, credentials, database files, runtime uploads and build output are excluded from the new repository.

Visual browser layout verification was unavailable: neither the browser automation surface nor the native computer-use runtime was connected. Component/API checks do not establish visual layout correctness. No frontend layout or styling was changed for this task.
