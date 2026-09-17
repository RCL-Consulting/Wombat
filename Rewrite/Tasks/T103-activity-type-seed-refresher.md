# T103 — Seeded activity-type JSON edits are inert on an existing database (T098 phase 2b)

**Status:** open
**Surfaced:** 2026-09-16 as a T098 note; specified 2026-09-17.
**Severity:** Medium — blocks every future seed change, including T070's `editable_by` declarations and the
ten remaining v11.1 tools.

## Symptom

Editing `src/Wombat.Infrastructure/Activities/Seeds/<key>/schema.json` (or `workflow.json`, `credit.json`)
changes nothing on a database where that activity type already exists. The app starts, the seeder runs, the
old schema stays. There is no error and no log line — the change simply does not happen.

## Root cause

Both seeders skip keys that already exist:

- `PaediatricCatalogueSeeder.cs:113` — skips an `ActivityType` whose `Key` is present.
- `DataSeeder.cs:248-251` — the same behaviour for the ten generic types.

This is correct for idempotency and wrong for evolution. `ActivityType.Key` is globally unique
(`ActivityTypeConfiguration.cs:28`), so a collision is a silent no-op rather than an error — which is how
T098 phase 2a's first attempt seeded only two of four tools before it was noticed.

The consequence compounds: once a type exists, the **only** path to a new schema version is publishing by
hand through the T054/T019 builder UI, per environment, forever.

## What to build

`ActivityTypeSeedRefresher`, running at startup after the existing seeders:

1. For each seed folder, parse the on-disk JSON through `FormSchemaParser` / `WorkflowParser` /
   `CreditRulesParser` and **re-serialise** it, producing a canonical string.
2. Load the stored `ActivityType`, canonicalise its current JSON the same way, and compare.
3. When they differ **and** `OwnerUserId` is the seed identity **and** `!HasDraft`, call `SaveDraft` then
   `PublishDraft` to create a new version.

Guards that matter:

- **Idempotency is the whole ballgame.** The comparison must be canonical-to-canonical, not raw-string, or
  every boot bumps `Version` forever. Test: run the refresher twice, assert `Version` is unchanged the
  second time.
- **Never overwrite operator work.** If `OwnerUserId` is not the seed identity, or a draft is in flight,
  skip and log — an institution that customised a seeded type must not have it reverted at startup.
- **In-flight activities stay pinned.** `ActivityService.cs:172` resolves the pinned `SchemaVersion`, so an
  activity created against version 1 keeps validating against version 1 after a bump. That is correct
  behaviour, but it means a re-publish does **not** unblock an already-created activity — say so in the log
  and in the release note.
- The round-trip is load-bearing: `ActivityType.SaveDraft` (`ActivityType.cs:51-52`) itself does
  Parse + Serialize, so any DSL property whose `Serialize` half is missing is silently dropped at publish.
  A round-trip test per DSL property is the cheap guard, and it should be written **before** the refresher.

## Why it is needed now

- T070 adds `editable_by` to the four `_cpsa` seed files. Without this refresher those declarations reach a
  **fresh** database only; dev and production keep the old pinned versions.
- Seeding the ten remaining v11.1 tools (T098 phase 2c) means iterating on seed JSON, which is painful to
  the point of impractical without it.

## Verification

- Edit a seeded schema's help text, restart: the type gains a version, the new text is live, the old version
  still exists and in-flight activities still validate against it.
- Restart again with no edit: no version bump (assert on `Version` before and after).
- Mark a type as operator-owned, edit the seed, restart: skipped, with a log line naming the type.

## Related

T098 phase 2b. Prerequisite for T070's seed half reaching existing databases, and for seeding the remaining
ten tools.
