---
id: T229
title: DataSeeder finds the demo College and institution by short codes an administrator can edit, so a changed code stops startup
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
---

# T229 — DataSeeder finds the demo College and institution by short codes an administrator can edit, so a changed code stops startup

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. `DataSeeder` runs in every environment (`Program.cs`), and an ordinary admin edit stops the next boot.
**Surfaced:** 2026-09-25, the T221 review. It predates T221, which fixed the same defect in `PaediatricCatalogueSeeder`.
Found by reading the code; not reproduced.

## Symptom

- `DataSeeder` finds the Demo Institution by `ShortCode == "DEMO"` and the Demo College by
  `SingleAsync(ShortCode == "DEMO-C")`. Both short codes are editable (`UpdateInstitutionCommandHandler`,
  `UpdateCollege`).
- **If DEMO-C changes,** `SingleAsync` throws and startup fails.
- **If DEMO changes,** the seeder creates a second "Demo College" and "Demo Institution", the unique name indexes refuse
  them, and startup fails.
- `DevUserSeeder` looks rows up by editable values too, but it skips on a miss, which is harmless.

## What to build

The same as [T221]: find the seeded rows by a seed key (T221 added `SeedKey` columns to the catalogue tables; extend it
to institutions and the demo rows), or warn and skip when the rows are missing but the demo data exists. Never create a
second set, and never fail startup.

## Verification

- [ ] Changing each short code, then booting, creates nothing and logs at most a warning. Postgres test per row.
- [ ] A fresh database still seeds the demo data once.

## Related

T221, T187, T174.
