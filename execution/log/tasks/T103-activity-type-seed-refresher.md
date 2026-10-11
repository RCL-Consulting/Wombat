---
id: T103
title: "Seeded activity-type JSON edits are inert on an existing database (T098 phase 2b)"
status: done
priority: P2
created: 2026-09-16
---
# T103 — Seeded activity-type JSON edits are inert on an existing database (T098 phase 2b)

**Status:** done - 2026-09-17
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

---

## Implementation record — 2026-09-17

`ActivityTypeSeedRefresher` (`src/Wombat.Infrastructure/Persistence/ActivityTypeSeedRefresher.cs`) runs
at startup after both seeders (`Program.cs`), reads the fourteen seed folders, and republishes the ones
whose on-disk JSON no longer matches what is published — provided three ownership guards pass.

### The comparison is canonical-to-canonical, and it has to be

The four JSON columns are `jsonb`. PostgreSQL parses what it is handed, discards the byte string, and
renders its own text on read: object keys reordered (shortest first, then bytewise), `", "` and `": "`
separators inserted. **Nothing stored in those columns is ever byte-equal to `FormSchemaParser.Serialize`
output**, not even for a seed file that has never changed. A raw-string comparison — or a comparison of the
stored string against a freshly serialised one — would therefore republish all fourteen types on *every
boot*, forever.

So both sides go through Parse + Serialize before they are compared
(`ActivityTypeSeedCatalogue.Canonicalise`). This is safe because canonicalisation is a fixed point, which
`SeedRoundTripTests` asserts per file per DSL.

There is no one-time normalising bump: the five types whose seed folders have not changed since they were
seeded compare equal on the first run and are not touched at all.

### Three guards, all evaluated before `SaveDraft`

`SaveDraft` overwrites all six staging columns unconditionally, so a guard that ran afterwards would
already have destroyed an operator's draft.

1. `OwnerUserId == "seed-system"`.
2. `!HasDraft`.
3. **The newest `ActivityTypeVersion.PublishedByUserId` is `"seed-system"`.** This is the guard that
   actually works. `SaveActivityTypeDraftCommand` assigns `OwnerUserId` only on the *new type* branch —
   editing an existing type never reassigns it — so a College or Institutional admin who customises and
   publishes a seeded type still reads as seed-owned forever. `PublishedByUserId` is the honest signal.
   Guard 1 alone would have reverted their work at the next boot, which is precisely what this task's
   guard exists to prevent.

A fourth check is not an ownership guard but a safety one: a type whose current `Version` has **no**
matching `ActivityTypeVersion` row is skipped with a warning rather than bumped. Bumping would append
vN+1 and leave every activity pinned to vN exactly as broken — `ActivityService.GetPinnedVersion` throws
on every read and write for them. That shape is reachable on any database predating the
`ActivityTypeDrafts` migration, which created the versions table with no backfill.

### Display fields are per-seeder, not uniform

`DisplayFieldsJson` is a fourth publish input and the two seeders disagree: `DataSeeder` derives it from
the first three schema field keys, `PaediatricCatalogueSeeder` passes `[]`. A single rule in the refresher
would have silently rewritten one group or flip-flopped both on every boot. The rule now travels with the
key in `ActivityTypeSeedCatalogue`, which both seeders and the refresher read — this also collapses the
two duplicate `SeedActorUserId` constants into one.

### It cannot take the host down

`Program.cs` has no try/catch around the seed block, systemd is `Restart=always` / `RestartSec=5`, and
`wombat-health.sh` restarts on top of that — so a throwing startup component is a full outage, not a
degraded feature. The refresher catches everything at the top level, and catches per type around reading
and parsing, so one bad seed folder (an interrupted rsync, a malformed edit) skips that type and leaves
the other thirteen alone. It also takes a `pg_advisory_lock` for the duration, so two hosts booting
together cannot both bump to the same version and collide on the unique `(ActivityTypeId, Version)` index.

### Kill switch

`Wombat__RefreshSeededActivityTypes=false` turns the republish off. The diff still runs and still logs,
one line per differing key — so a stale seed becomes a *visible* no-op instead of the silent one this task
was filed for. Worth setting around a rollback: **a binary rollback does not undo a version bump.** After
`mv app.prev app && systemctl restart`, the DB holds v2 with new content while the rolled-back binary
ships the old seed files, whose refresher diffs and publishes v3 with the old content.

### Release note — what a production boot will actually do

Expected first run on any database seeded before T070 (verified against dev by a scout; **production was
not inspected and should be counted first**):

- **Unchanged (5):** `journal_club`, `procedure_log`, `qi_project`, `reflective_note`, `teaching_session`.
- **Republished to v2 (9):** `acat`, `cbd`, `cbd_cpsa`, `direct_observation_cpsa`, `dops`, `dops_cpsa`,
  `mini_cex`, `mini_cex_cpsa`, `research_output` — all T070 content.
- **Not in the catalogue, untouched:** `mini_cex_paed`, `dops_paed`, `procedure_log_paed`, `msf_paed`
  (operator-built, no seed folder).
- Second boot: zero changes, zero UPDATEs.

Three consequences to state plainly:

1. **In-flight activities are not unblocked.** They stay pinned to their old version, which is correct and
   is logged on every republish. Browser-verifying T070 after this lands needs a **freshly created**
   activity — an existing CPSA Mini-CEX still validates against its v1 and still cannot take a rating.
2. **T070 removed `terminal: true` from `declined`/`cancelled` (four legacy WBAs) and from
   `research_output`'s `rejected`.** Republishing makes that live. It is deliberate — a terminal
   abandonment state awarded curriculum credit for a refused assessment — but it changes terminal-state
   semantics, and `CreditApplier` fires on terminal states.
3. `AssessorPendingNudgeJob` reads the **live** `ActivityType.WorkflowJson`, not the pinned version. None
   of the nine republishes rename or remove a state key, so nudges are unaffected this time. A future seed
   edit that renamed a state would silently stop nudges for activities pinned to the old version — worth a
   follow-up task, not fixed here.

### Tests

- `tests/Wombat.Infrastructure.Tests/Activities/SeedRoundTripTests.cs` — **written first**, as the task
  asks. Per seed folder, per DSL: canonicalisation is a fixed point, and every property on disk survives
  Parse + Serialize. The second assertion is the guard for a DSL property with a `Parse` half and no
  `Serialize` half, which would otherwise be dropped at publish *and then be invisible forever*, because a
  canonical-to-canonical comparison drops it on both sides and reads as "in sync". The no-loss check is
  itself tested against a hand-made dropped property. The corpus round-trips losslessly today, including
  T070's `editable_by` at all three levels.
- `tests/Wombat.Infrastructure.Tests/Activities/ActivityTypeSeedRefresherTests.cs` — idempotency (run
  twice, version unchanged), and the one that matters:
  `Refresh_IsIdempotent_WhenStoredJsonIsRenderedTheWayPostgresRendersJsonb` writes the PostgreSQL `jsonb`
  rendering into the column deliberately, because the in-memory provider stores strings verbatim and would
  pass a naive raw-string comparison while production flapped. Plus each guard, the disabled-mode report,
  the audit entry, the per-seeder display-fields rule, and a bad-JSON type not stopping the other thirteen.

### Not done here

- The refresher evolves the four versioned JSON payloads only. `Name`, `Description` and `Scope` are not
  refreshed — they are unversioned, operator-editable, and have no guard.
- Nothing fixes the underlying collision hazard: a seed key that collides with an operator-built type is
  still a silent no-op in the seeders (the `mini_cex_paed` / `dops_paed` mess).
- There is no integration test against real PostgreSQL. The repo has no Testcontainers; the one integration
  test uses a live local server with a per-test schema. The jsonb failure mode is covered by simulating
  PostgreSQL's rendering exactly, which is hermetic — but a real-database run of the refresher twice is
  still the honest final check before production.

### Adversarial review — four fixes, and the run against the real database

Three reviewers attacked the finished diff (idempotency, live-data safety, does-it-actually-work). The
canonicalisation and the ownership guards held. Four defects were substantiated and fixed:

1. **No per-type error isolation in the publish phase (medium).** Each type commits with its own
   `SaveChangesAsync`, but only the read/parse phase was wrapped. A failure on type six left types one to
   five republished while the only line an operator saw read "Published activity types are unchanged".
   Each type is now its own try/catch, and the top-level message no longer claims nothing changed.
2. **A failed save left mutated entities attached to the shared startup DbContext (low, but nasty).**
   `SaveDraft`/`PublishDraft` mutate in place — `Version++`, staging cleared, a version row added to the
   navigation. After a failed save those changes sat in the change tracker of a context `Program.cs` goes
   on to hand to `DevUserSeeder`, so the next unrelated save could commit a refresh reported as failed.
   `RollbackTrackedChanges` now detaches added entities and reloads the type.
3. **`pg_advisory_lock` is blocking, with no timeout and no cancellation (low).** The class documents "it
   cannot take the host down", which covered exceptions but not an indefinite wait — an orphaned backend
   or a DBA session holding the same key would hang the boot before the host ever listened. Now
   `pg_try_advisory_lock`: failing to take it means another host is refreshing, which is a reason to skip
   and log, not to wait.
4. **The summary omitted two outcomes (low).** `NotSeededYet` produced no per-type line *and* no counter,
   so a database where a seeder never completed looked identical to a healthy one while the counters
   silently failed to add up to the printed total. Both it and the new failure count are now in the
   summary, and the line logs at Error when anything failed.

### Verified against the dev database

Two boots, with a `pre-t103-refresh` snapshot taken first:

```
boot 1:  9 republished, 5 unchanged, 0 skipped (0 of them failed), 0 pending, 0 not yet seeded, of 14
boot 2:  0 republished, 14 unchanged, 0 skipped (0 of them failed), 0 pending, 0 not yet seeded, of 14
```

Exactly the scout's prediction. Post-state confirmed by query: `mini_cex_cpsa` at v2 carries
`editable_by: field:assessor_user_id` on the `requested` state; `mini_cex`/`dops`/`cbd`/`acat`/
`research_output` at v2; the five unchanged types still at v1; the four operator-built `*_paed` types
untouched; all 18 types have an `ActivityTypeVersion` row matching their current `Version`.

### The remaining dead end — filed as T107

Existing activities stay pinned to their old version, which is correct. But `ActivityView` still renders a
**Complete** button for the bound assessor on those activities, and it can never succeed: the pinned v1
has no `editable_by`, so the writable set is empty, the form renders read-only, and the transition throws
the required-fields error. There is no re-pin path anywhere — `Activity.SchemaVersion` is assigned in
exactly one place, at creation. See **T107**.
