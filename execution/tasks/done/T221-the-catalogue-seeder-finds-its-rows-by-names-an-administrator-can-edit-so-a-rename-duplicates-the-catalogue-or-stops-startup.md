---
id: T221
title: The catalogue seeder finds its rows by names an administrator can edit, so a rename duplicates the catalogue or stops startup
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T221 — The catalogue seeder finds its rows by names an administrator can edit, so a rename duplicates the catalogue or stops startup

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. A routine admin edit (renaming the sub-speciality, the College's short code or the v11.1 scale)
changes what the next boot does to the national catalogue.
**Surfaced:** 2026-09-25, the T187 review. It predates T187. Found by reading the code; not reproduced.

## Symptom

`PaediatricCatalogueSeeder` looks each catalogue row up by a value an admin can edit, and creates the row when the
lookup misses:

| Row | Looked up by | Edited at |
|---|---|---|
| College | `ShortCode == "CPSA"` | `UpdateCollege` |
| Speciality | `(CollegeId, Name == "Paediatrics")` | `UpdateSpecialityCommandHandler` |
| Sub-speciality | `(SpecialityId, Name == "Paediatrics")` | `UpdateSubSpecialityCommandHandler` (SubSpecialityEdit) |
| v11.1 scale | `Name` | `UpdateEntrustmentScaleCommandHandler` |

- **Rename the sub-speciality:** the next boot creates a second "Paediatrics" sub-speciality, 15 duplicate College EPAs
  and a second v11.1 curriculum.
- **Change CPSA's short code but keep its name:** the next boot inserts a college with the same name. That breaks the
  unique index on `Colleges.Name`, nothing catches it around `PaediatricCatalogueSeeder.SeedAsync()` in `Program.cs`,
  and startup fails.
- **Rename the v11.1 scale:** the next boot creates a second v11.1 ladder, and T187's and T174's warnings fire against
  the wrong rows.

## What to build

Find the seeded rows by something an admin cannot edit: a seed key stored on the row, or the ids the first seeding
recorded. If the catalogue exists but a row cannot be found, warn and skip; never create a second catalogue. W-007
applies: a migration that adds a seed key and stamps the existing rows is fine.

## Verification

- [x] Renaming each of the four rows, then booting, creates nothing and logs at most a warning. Postgres test per row.
- [x] A fresh database still seeds the whole catalogue once. Existing seed tests stay green.

## Related

T187, T174, T122, T130.

---

## As built — 2026-09-25 (`62447ad`)

`PaediatricCatalogueSeeder` finds its catalogue rows by a `SeedKey` column: the College, speciality, sub-speciality,
v11.1 scale, 15 EPAs and curriculum. The keys are unique where not null, and existing rows were stamped by the migration
`T221_CatalogueSeedKeys`. Once the catalogue exists, a row it cannot find is warned about and skipped, never created, and
the first seeding is one save. Postgres tests cover each rename, a fresh database, and a first boot that fails part-way.
CLAUDE.md says later catalogue additions reach an existing database only through a migration.

Browser on dev (scripted Chrome, master `d6b2796`; `pg_dump` first, at `recovery/pre-g1-migrations.dump`):
- **The first boot** logged no seed-key warnings. There are 20 keys: 1 each for the College, speciality, sub-speciality
  and scale, 15 EPAs and 1 curriculum.
- **Renames, as collegeadmin.** The sub-speciality was renamed and PAED-015's code changed, then the app was restarted.
  There were no warnings and no new rows (Colleges 2, SubSpecialities 2, Epas 17, Curricula 2), and both rows kept
  their keys. Both were reverted.
- **Not run in a browser:** the College short-code change, because `CollegeEdit` is Administrator-only. It is covered by
  its Postgres test.

**Filed from the review:** [T229] (P2): `DataSeeder`'s demo rows have the same defect.
