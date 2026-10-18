# Seed integration verification

Run from `Backend` with .NET 9 and SQL Server LocalDB installed:

```powershell
dotnet run --project scripts/SeedVerification
```

The console verifier uses the same API design-time DbContext factory as `Update-Database`.
It creates three uniquely named `MatarzatSeedVerification_<guid>_*` databases and an isolated
temporary upload directory. It never accepts or uses an existing configured database.

Checks cover clean synchronous and asynchronous migrations, the complete bilingual catalogue,
local image installation, product/variant/inventory foreign keys, unchanged repeat migrations,
concurrent seed/migration execution, merchant edits and unrelated data, existing password hashes
and role membership, soft deletion, Production environment selection (including forwarded
`--environment Production` arguments), and the explicit disabled Development seed flag.

Full-table fingerprints include every persisted column, including inventory row versions and
timestamps, so preservation checks detect unwanted updates as well as inserts and deletions.
Test databases and uploads are removed only after checking their generated names and LocalDB
server. Retain them for diagnosis with:

```powershell
dotnet run --project scripts/SeedVerification -- --keep-databases
```

No credentials are printed or written to files. The verifier creates an ephemeral password
through the application's existing password hasher solely to test account preservation.
