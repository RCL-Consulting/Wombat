# T028 — Rename `star_reflection` activity type to `reflective_note`

**Plan:** `Rewrite/practical-plan.md` — Block 1, first task
**Depends on:** T020 (seeded activity types)
**Blocks:** T029 (`EntrustmentDecision` aggregate — needs the STAR name free)
**Model:** Sonnet (mechanical rename + data migration)

## Goal

Free the STAR acronym for the real Statement of Awarded Responsibility aggregate coming in T029. The seeded reflection activity type currently uses `star_reflection` as its key, which collides with STAR's regulatory meaning. Rename the activity type key to `reflective_note`.

## Why this first

T029 introduces the formal STAR artefact. If `star_reflection` still owns the acronym when T029 ships, every reference — code, UI, admin documentation, support conversations — has to be disambiguated. Cheaper to rename now than after T029.

## What to do

### 1. Seed source

**`src/Wombat.Infrastructure/Persistence/DataSeeder.cs`** — line 20 tuple. Replace:
```csharp
new("star_reflection", "STAR Reflection", "Situation-task-action-result reflection.", ActivityScope.Speciality),
```
with:
```csharp
new("reflective_note", "Reflective Note", "Structured reflective note using the situation-task-action-result frame.", ActivityScope.Speciality),
```

### 2. Seed files

Rename the directory `src/Wombat.Infrastructure/Activities/Seeds/star_reflection/` to `src/Wombat.Infrastructure/Activities/Seeds/reflective_note/`. The directory's three files (`schema.json`, `workflow.json`, `credit.json`) need no content changes — none embed the key name.

Verify by grepping the new directory for `star_reflection` after rename — should return zero matches.

### 3. Seed README

**`src/Wombat.Infrastructure/Activities/Seeds/README.md`** — line 15. Replace the bullet referencing `star_reflection` with one for `reflective_note`. Adjust description wording to match the new title.

### 4. Data migration

New migration under `src/Wombat.Infrastructure/Persistence/Migrations/`. Timestamp convention per prior migrations (`YYYYMMDDHHMMSS_RenameStarReflection`).

The migration must:
- `UPDATE ActivityTypes SET Key = 'reflective_note', Title = 'Reflective Note' WHERE Key = 'star_reflection';`
- Down-migration reverses the rename.
- Include the `.Designer.cs` file (see CLAUDE.md — hand-written migrations fail silently without it).
- Update `ApplicationDbContextModelSnapshot.cs` if the model state reflects anything key-related (it shouldn't — key is a string column, not part of the schema model shape).

`ActivityPermissionRule` references by `ActivityTypeId`, not by key — no updates needed there. Confirmed against `src/Wombat.Domain/Activities/ActivityPermissionRule.cs`.

### 5. Display label audit

Grep the Web project for any hard-coded `"STAR Reflection"` or `"star_reflection"` strings. If found in:
- Component files (`.razor`) — replace with `"Reflective Note"` / `"reflective_note"`
- Resource strings — update.
- Tests — update fixtures.

Expected: the Web layer should query by key or label from the DB, not hard-code. Audit to confirm.

### 6. Tests

- Architecture test: still passes (no new project references).
- Domain/Application tests: run to confirm no fixtures reference the old key.
- Add one integration test (or assertion in existing seeder test) confirming `reflective_note` is the seeded key after running `DataSeeder`.

### 7. Documentation

Update any `Rewrite/*.md` files that reference `star_reflection` by name. Known hits from prior grep:
- `Rewrite/DOMAIN.md`
- `Rewrite/CUSTOMIZATION.md`
- `Rewrite/practical-plan.md` (already pre-written with the new name, verify)
- `EPA Book/evaluation.md` — evaluation commentary, leave as historical narrative but add a footnote noting the rename.

## Validation

Before declaring done:
```bash
dotnet build Wombat.sln -c Release           # zero errors, zero warnings
dotnet test                                   # all green
```

Grep the solution:
```
Grep: "star_reflection"
```
Expected matches: only the migration file (down-migration path) and historical documents in `EPA Book/` or `Rewrite/book-fidelity-plan.md` (reference, not authoritative).

Start the Web host, log in as SpecialityAdmin, navigate to the activity types admin page, confirm:
- "Reflective Note" appears in the list
- "STAR Reflection" does not appear
- An existing activity (if any seeded) opens correctly against the renamed type

## Exit criteria

- Build + tests green
- Database has `ActivityTypes.Key = 'reflective_note'` for the renamed row
- Seed directory renamed
- No code or authoritative doc references `star_reflection` except in the migration and historical-reference docs
- Commit created, `current_state.md` advanced to T029

## Estimated effort

½ day.
